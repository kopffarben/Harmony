using Harmony.ILCopying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Harmony
{
	/// <summary>Defines a matching condition for a <see cref="CodeInstruction"/></summary>
	/// 
	public class CodeMatch
	{
		/// <summary>The name <see cref="CodeMatcher"/> uses to refer to this match</summary>
		/// 
		public readonly string Name = null;

		private List<OpCode> opcodes = new List<OpCode>();
		private List<object> operands = new List<object>();
		private List<Label> labels = new List<Label>();
		private List<ExceptionBlock> blocks = new List<ExceptionBlock>();

		private List<int> jumpsFrom = new List<int>();
		private List<int> jumpsTo = new List<int>();

		private readonly Func<CodeInstruction, bool> predicate = null;

		/// <summary>Constructor creating a <see cref="CodeMatch"/>. Use without opcode/operand to match any instruction</summary>
		/// 
		/// <param name="opcode"> (Optional) The <see cref="System.Reflection.Emit.OpCode" />. Use <see langword="null"/> to ignore the opcode</param>
		/// <param name="operand">(Optional) The operand. Use <see langword="null"/> to ignore the operand</param>
		/// <param name="name">	  (Optional) The name under this match is saved</param>
		///
		public CodeMatch(OpCode? opcode = null, object operand = null, string name = null)
		{
			if (opcode is OpCode opcodeValue) opcodes.Add(opcodeValue);
			if (operand != null) operands.Add(operand);
			Name = name;
		}

		/// <summary>Constructor creating a <see cref="CodeMatch"/> using multiple opcodes</summary>
		/// <param name="opcodes">A list of possible <see cref="System.Reflection.Emit.OpCode" /></param>
		/// <param name="operand">(Optional) The operand. Use <see langword="null"/> to ignore the operand.</param>
		/// <param name="name">	  (Optional) The name under this match is saved.</param>
		///
		public CodeMatch(List<OpCode> opcodes, object operand = null, string name = null)
		{
			opcodes.AddRange(opcodes);
			if (operand != null) operands.Add(operand);
			Name = name;
		}

		/// <summary>Constructor creating a <see cref="CodeMatch"/> using a <see cref="CodeInstruction"/></summary>
		/// <param name="instruction">The <see cref="CodeInstruction"/> to match</param>
		/// <param name="name">	  (Optional) The name under this match is saved</param>
		///
		public CodeMatch(CodeInstruction instruction, string name = null)
			: this(instruction.opcode, instruction.operand, name) { }

		/// <summary>Constructor creating a <see cref="CodeMatch"/> using a predicate</summary>
		/// <param name="predicate">The predicate function that determines the match</param>
		/// <param name="name">	  (Optional) The name under this match is saved</param>
		///
		public CodeMatch(Func<CodeInstruction, bool> predicate, string name = null)
		{
			this.predicate = predicate;
			Name = name;
		}

		internal bool Matches(List<CodeInstruction> instructions, CodeInstruction instruction)
		{
			if (predicate != null) return predicate(instruction);

			if (opcodes.Count > 0 && opcodes.Contains(instruction.opcode) == false) return false;
			if (operands.Count > 0 && operands.Contains(instruction.operand) == false) return false;
			if (labels.Count > 0 && labels.Intersect(instruction.labels).Any() == false) return false;
			if (blocks.Count > 0 && blocks.Intersect(instruction.blocks).Any() == false) return false;

			if (jumpsFrom.Count > 0 && jumpsFrom.Select(index => instructions[index].operand).OfType<Label>()
				.Intersect(instruction.labels).Any() == false) return false;

			if (jumpsTo.Count > 0)
			{
				var operand = instruction.operand;
				if (operand == null || operand.GetType() != typeof(Label)) return false;
				var label = (Label)operand;
				var indices = Enumerable.Range(0, instructions.Count).Where(idx => instructions[idx].labels.Contains(label));
				if (jumpsTo.Intersect(indices).Any() == false) return false;
			}

			return true;
		}

		/// <summary>Returns a string representation</summary>
		/// <returns>A string that represents the <see cref="CodeMatch"/></returns>
		///
		public override string ToString()
		{
			var result = "[";
			if (Name != null)
				result += Name + ": ";
			if (opcodes.Count > 0)
				result += "opcodes=" + opcodes.Join() + " ";
			if (operands.Count > 0)
				result += "operands=" + operands.Join() + " ";
			if (labels.Count > 0)
				result += "labels=" + labels.Join() + " ";
			if (blocks.Count > 0)
				result += "blocks=" + blocks.Join() + " ";
			if (jumpsFrom.Count > 0)
				result += "jumpsFrom=" + jumpsFrom.Join() + " ";
			if (jumpsTo.Count > 0)
				result += "jumpsTo=" + jumpsTo.Join() + " ";
			if (predicate != null)
				result += "predicate=yes ";
			return result.TrimEnd() + "]";
		}
	}

	/// <summary>A chainable class to do search/replace operations on list of <see cref="CodeInstruction"/>. Usually from within a transpiler</summary>
	public class CodeMatcher
	{
		private readonly ILGenerator generator;
		private readonly List<CodeInstruction> codes = new List<CodeInstruction>();
		private Dictionary<string, CodeInstruction> lastMatches = new Dictionary<string, CodeInstruction>();
		private string lastError = null;
		private bool lastUseEnd = false;
		private CodeMatch[] lastCodeMatches = null;

		private void FixStart() { Pos = Math.Max(0, Pos); }
		private void SetOutOfBounds(int direction) { Pos = direction > 0 ? Length : -1; }

		/// <summary>Gets the current index position</summary>
		/// <value>Current position. -1 means "before start" and Count+1 means "after end"</value>
		///
		public int Pos { get; private set; } = -1;

		/// <summary>Gets the total number of <see cref="CodeInstruction"/>s</summary>
		/// <value>The count</value>
		///
		public int Length => codes.Count;

		/// <summary>Tests if the current position is valid (0 - length-1)</summary>
		/// <value>True if valid, false if not valid</value>
		///
		public bool IsValid => Pos >= 0 && Pos < Length;

		/// <summary>Tests if the current position is invalid (less than 0 or greater or equal to length)</summary>
		/// <value>True if invalid, false if valid</value>
		///
		public bool IsInvalid => Pos < 0 || Pos >= Length;

		/// <summary>Gets the number of remaining <see cref="CodeInstruction"/>s</summary>
		/// <value>The remainder count</value>
		///
		public int Remaining => Length - Math.Max(0, Pos);

		/// <summary>Gets the opcode of the current <see cref="CodeInstruction"/></summary>
		/// <value>The current <see cref="System.Reflection.Emit.OpCode" /></value>
		///
		public ref OpCode Opcode => ref codes[Pos].opcode;

		/// <summary>Gets the operand of the current <see cref="CodeInstruction"/></summary>
		/// <value>The current operand (type depends on the opcode)</value>
		///
		public ref object Operand => ref codes[Pos].operand;

		/// <summary>Gets the labels of the current <see cref="CodeInstruction"/></summary>
		/// <value>The current <see cref="List{Label}"/></value>
		///
		public ref List<Label> Labels => ref codes[Pos].labels;

		/// <summary>Gets the exception block settings of the current <see cref="CodeInstruction"/></summary>
		/// <value>The current <see cref="List{ExceptionBlock}"/></value>
		///
		public ref List<ExceptionBlock> Blocks => ref codes[Pos].blocks;

		/// <summary>Default constructor</summary>
		public CodeMatcher()
		{
		}

		/// <summary>Constructor creating a <see cref="CodeMatcher"/> using the values available in a transpiler call</summary>
		/// <param name="instructions">The instructions.</param>
		/// <param name="generator">	 (Optional) The generator.</param>
		///
		public CodeMatcher(IEnumerable<CodeInstruction> instructions, ILGenerator generator = null)
		{
			this.generator = generator;
			codes = instructions.Select(c => new CodeInstruction(c)).ToList();
		}

		/// <summary>Creates a <see cref="CodeMatcher"/> by making a deep copy</summary>
		/// <returns>A copy of a <see cref="CodeMatcher"/></returns>
		///
		public CodeMatcher Clone()
		{
			return new CodeMatcher(codes, generator)
			{
				Pos = Pos,
				lastMatches = lastMatches,
				lastError = lastError,
				lastUseEnd = lastUseEnd,
				lastCodeMatches = lastCodeMatches
			};
		}

		/// <summary>	reading instructions out ---------------------------------------------. </summary>
		/// <value>	The instruction. </value>
		///
		public CodeInstruction Instruction => codes[Pos];

		/// <summary>	Instruction at. </summary>
		/// <param name="offset">	The offset.</param>
		/// <returns>	A CodeInstruction. </returns>
		///
		public CodeInstruction InstructionAt(int offset) => codes[Pos + offset];

		/// <summary>	Gets the instructions. </summary>
		/// <returns>	A List&lt;CodeInstruction&gt; </returns>
		///
		public List<CodeInstruction> Instructions() => codes;

		/// <summary>	Gets the instructions. </summary>
		/// <param name="count">	Number of.</param>
		/// <returns>	A List&lt;CodeInstruction&gt; </returns>
		///
		public List<CodeInstruction> Instructions(int count)
			=> codes.GetRange(Pos, count).Select(c => new CodeInstruction(c)).ToList();

		/// <summary>	Instructions in range. </summary>
		/// <param name="start">	The start.</param>
		/// <param name="end">  	The end.</param>
		/// <returns>	A List&lt;CodeInstruction&gt; </returns>
		///
		public List<CodeInstruction> InstructionsInRange(int start, int end)
		{
			var instructions = codes;
			if (start > end) { var tmp = start; start = end; end = tmp; }
			instructions = instructions.GetRange(start, end - start + 1);
			return instructions.Select(c => new CodeInstruction(c)).ToList();
		}

		/// <summary>	Instructions with offsets. </summary>
		/// <param name="startOffset">	The start offset.</param>
		/// <param name="endOffset">  	The end offset.</param>
		/// <returns>	A List&lt;CodeInstruction&gt; </returns>
		///
		public List<CodeInstruction> InstructionsWithOffsets(int startOffset, int endOffset)
			=> InstructionsInRange(Pos + startOffset, Pos + endOffset);

		/// <summary>	Distinct labels. </summary>
		/// <param name="instructions">	The instructions.</param>
		/// <returns>	A List&lt;Label&gt; </returns>
		///
		public List<Label> DistinctLabels(IEnumerable<CodeInstruction> instructions)
			=> instructions.SelectMany(instruction => instruction.labels).Distinct().ToList();

		/// <summary>	Reports a failure. </summary>
		/// <param name="method">	The method.</param>
		/// <param name="logger">	The logger.</param>
		/// <returns>	True if it succeeds, false if it fails. </returns>
		///
		public bool ReportFailure(MethodBase method, Action<string> logger)
		{
			if (IsValid) return false;
			var err = lastError ?? "Unexpected code";
			logger(err + " in " + method);
			return true;
		}

		/// <summary>	edit operation -------------------------------------------------------. </summary>
		/// <param name="instruction">	The instruction.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher SetInstruction(CodeInstruction instruction)
		{
			codes[Pos] = instruction;
			return this;
		}

		/// <summary>	Sets instruction and advance. </summary>
		/// <param name="instruction">	The instruction.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher SetInstructionAndAdvance(CodeInstruction instruction)
		{
			SetInstruction(instruction);
			Pos++;
			return this;
		}

		/// <summary>	Sets. </summary>
		/// <param name="opcode"> 	The opcode.</param>
		/// <param name="operand">	The operand.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher Set(OpCode opcode, object operand)
		{
			Opcode = opcode;
			Operand = operand;
			return this;
		}

		/// <summary>	Sets and advance. </summary>
		/// <param name="opcode"> 	The opcode.</param>
		/// <param name="operand">	The operand.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher SetAndAdvance(OpCode opcode, object operand)
		{
			Set(opcode, operand);
			Pos++;
			return this;
		}

		/// <summary>	Sets opcode and advance. </summary>
		/// <param name="opcode">	The opcode.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher SetOpcodeAndAdvance(OpCode opcode)
		{
			Opcode = opcode;
			Pos++;
			return this;
		}

		/// <summary>	Sets operand advance. </summary>
		/// <param name="operand">	The operand.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher SetOperandAndAdvance(object operand)
		{
			Operand = operand;
			Pos++;
			return this;
		}

		/// <summary>	Creates a label. </summary>
		/// <param name="label">	[out] The label.</param>
		/// <returns>	The new label. </returns>
		///
		public CodeMatcher CreateLabel(out Label label)
		{
			label = generator.DefineLabel();
			Labels.Add(label);
			return this;
		}

		/// <summary>	Creates label at. </summary>
		/// <param name="position">	The position.</param>
		/// <param name="label">		[out] The label.</param>
		/// <returns>	The new label at. </returns>
		///
		public CodeMatcher CreateLabelAt(int position, out Label label)
		{
			label = generator.DefineLabel();
			codes[position].labels.Add(label);
			return this;
		}

		/// <summary>	Adds the labels. </summary>
		/// <param name="labels">	The labels.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher AddLabels(IEnumerable<Label> labels)
		{
			Labels.AddRange(labels);
			return this;
		}

		/// <summary>	Adds the labels at to 'labels'. </summary>
		/// <param name="position">	The position.</param>
		/// <param name="labels">  	The labels.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher AddLabelsAt(int position, IEnumerable<Label> labels)
		{
			codes[position].labels.AddRange(labels);
			return this;
		}

		/// <summary>	Sets jump to. </summary>
		/// <param name="destination">	Destination for the.</param>
		/// <param name="label">			[out] The label.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher SetJumpTo(int destination, out Label label)
		{
			CreateLabelAt(destination, out label);
			Labels.Add(label);
			return this;
		}

		/// <summary>	insert operations ----------------------------------------------------. </summary>
		/// <param name="instructions">	The instructions.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher Insert(params CodeInstruction[] instructions)
		{
			codes.InsertRange(Pos, instructions);
			return this;
		}

		/// <summary>	insert operations ----------------------------------------------------. </summary>
		/// <param name="instructions">	The instructions.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher Insert(IEnumerable<CodeInstruction> instructions)
		{
			codes.InsertRange(Pos, instructions);
			return this;
		}

		/// <summary>	Inserts a branch. </summary>
		/// <param name="opcode">			The opcode.</param>
		/// <param name="destination">	Destination for the.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher InsertBranch(OpCode opcode, int destination)
		{
			CreateLabelAt(destination, out var label);
			codes.Insert(Pos, new CodeInstruction(opcode, label));
			return this;
		}

		/// <summary>	Inserts an and advance described by instructions. </summary>
		/// <param name="instructions">	The instructions.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher InsertAndAdvance(params CodeInstruction[] instructions)
		{
			instructions.Do(instruction =>
			{
				Insert(instruction);
				Pos++;
			});
			return this;
		}

		/// <summary>	Inserts an and advance described by instructions. </summary>
		/// <param name="instructions">	The instructions.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher InsertAndAdvance(IEnumerable<CodeInstruction> instructions)
		{
			instructions.Do(instruction => InsertAndAdvance(instruction));
			return this;
		}

		/// <summary>	Inserts a branch and advance. </summary>
		/// <param name="opcode">			The opcode.</param>
		/// <param name="destination">	Destination for the.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher InsertBranchAndAdvance(OpCode opcode, int destination)
		{
			InsertBranch(opcode, destination);
			Pos++;
			return this;
		}

		/// <summary>
		///   delete operations --------------------------------------------------------.
		/// </summary>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher RemoveInstruction()
		{
			codes.RemoveAt(Pos);
			return this;
		}

		/// <summary>	Removes the instructions described by count. </summary>
		/// <param name="count">	Number of.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher RemoveInstructions(int count)
		{
			codes.RemoveRange(Pos, Pos + count - 1);
			return this;
		}

		/// <summary>	Removes the instructions in range. </summary>
		/// <param name="start">	The start.</param>
		/// <param name="end">  	The end.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher RemoveInstructionsInRange(int start, int end)
		{
			if (start > end) { var tmp = start; start = end; end = tmp; }
			codes.RemoveRange(start, end - start + 1);
			return this;
		}

		/// <summary>	Removes the instructions with offsets. </summary>
		/// <param name="startOffset">	The start offset.</param>
		/// <param name="endOffset">  	The end offset.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher RemoveInstructionsWithOffsets(int startOffset, int endOffset)
		{
			RemoveInstructionsInRange(Pos + startOffset, Pos + endOffset);
			return this;
		}

		/// <summary>
		///   moving around ------------------------------------------------------------.
		/// </summary>
		/// <param name="offset">	The offset.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher Advance(int offset)
		{
			Pos += offset;
			if (IsValid == false) SetOutOfBounds(offset);
			return this;
		}

		/// <summary>	Gets the start. </summary>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher Start()
		{
			Pos = 0;
			return this;
		}

		/// <summary>	Gets the end. </summary>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher End()
		{
			Pos = Length - 1;
			return this;
		}

		/// <summary>	Searches for the first forward. </summary>
		/// <param name="predicate">	The predicate.</param>
		/// <returns>	The found forward. </returns>
		///
		public CodeMatcher SearchForward(Func<CodeInstruction, bool> predicate) => Search(predicate, 1);

		/// <summary>	Searches for the first back. </summary>
		/// <param name="predicate">	The predicate.</param>
		/// <returns>	The found back. </returns>
		///
		public CodeMatcher SearchBack(Func<CodeInstruction, bool> predicate) => Search(predicate, -1);

		/// <summary>	Searches for the first match. </summary>
		/// <param name="predicate">	The predicate.</param>
		/// <param name="direction">	The direction.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		private CodeMatcher Search(Func<CodeInstruction, bool> predicate, int direction)
		{
			FixStart();
			while (IsValid && predicate(Instruction) == false)
				Pos += direction;
			lastError = IsInvalid ? "Cannot find " + predicate : null;
			return this;
		}

		/// <summary>	Match forward. </summary>
		/// <param name="useEnd"> 	True to use end.</param>
		/// <param name="matches">	A variable-length parameters list containing matches.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher MatchForward(bool useEnd, params CodeMatch[] matches) => Match(matches, 1, useEnd);

		/// <summary>	Match back. </summary>
		/// <param name="useEnd"> 	True to use end.</param>
		/// <param name="matches">	A variable-length parameters list containing matches.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher MatchBack(bool useEnd, params CodeMatch[] matches) => Match(matches, -1, useEnd);

		/// <summary>	Matches. </summary>
		/// <param name="matches">  	A variable-length parameters list containing matches.</param>
		/// <param name="direction">	The direction.</param>
		/// <param name="useEnd">	 	True to use end.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		private CodeMatcher Match(CodeMatch[] matches, int direction, bool useEnd)
		{
			FixStart();
			while (IsValid)
			{
				lastUseEnd = useEnd;
				lastCodeMatches = matches;
				if (MatchSequence(Pos, matches))
				{
					if (useEnd) Pos += matches.Count() - 1;
					break;
				}
				Pos += direction;
			}
			lastError = IsInvalid ? "Cannot find " + matches.Join() : null;
			return this;
		}

		/// <summary>	Repeats. </summary>
		/// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
		/// <param name="matchAction">		The match action.</param>
		/// <param name="notFoundAction">	(Optional) The not found action.</param>
		/// <returns>	A CodeMatcher. </returns>
		///
		public CodeMatcher Repeat(Action<CodeMatcher> matchAction, Action<string> notFoundAction = null)
		{
			var count = 0;
			if (lastCodeMatches == null)
				throw new InvalidOperationException("No previous Match operation - cannot repeat");

			while (IsValid)
			{
				matchAction(this);
				MatchForward(lastUseEnd, lastCodeMatches);
				count++;
			}
			lastCodeMatches = null;

			if (count == 0 && notFoundAction != null)
				notFoundAction(lastError);

			return this;
		}

		/// <summary>	Named match. </summary>
		/// <param name="name">	The name.</param>
		/// <returns>	A CodeInstruction. </returns>
		///
		public CodeInstruction NamedMatch(string name)
			=> lastMatches[name];

		private bool MatchSequence(int start, CodeMatch[] matches)
		{
			if (start < 0) return false;
			lastMatches = new Dictionary<string, CodeInstruction>();
			foreach (var match in matches)
			{
				if (start >= Length || match.Matches(codes, codes[start]) == false)
					return false;
				if (match.Name != null)
					lastMatches.Add(match.Name, codes[start]);
				start++;
			}
			return true;
		}
	}
}