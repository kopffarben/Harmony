using Harmony.ILCopying;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Harmony
{
	/// <summary>A high level wrapper for a IL code and its operand</summary>
	public class CodeInstruction
	{
		/// <summary>The opcode.</summary>
		public OpCode opcode;

		/// <summary>The operand.</summary>
		public object operand;

		/// <summary>The labels.</summary>
		public List<Label> labels = new List<Label>();

		/// <summary>The blocks.</summary>
		public List<ExceptionBlock> blocks = new List<ExceptionBlock>();

		/// <summary>Constructor.</summary>
		/// <param name="opcode"> The opcode.</param>
		/// <param name="operand">(Optional) The operand.</param>
		///
		public CodeInstruction(OpCode opcode, object operand = null)
		{
			this.opcode = opcode;
			this.operand = operand;
		}

		/// <summary>
		///   full copy (be careful with duplicate labels and exception blocks!)
		///   for normal cases, use Clone()
		/// </summary>
		/// <param name="instruction">The instruction.</param>
		///
		public CodeInstruction(CodeInstruction instruction)
		{
			opcode = instruction.opcode;
			operand = instruction.operand;
			labels = instruction.labels.ToArray().ToList();
			blocks = instruction.blocks.ToArray().ToList();
		}

		/// <summary>copy only opcode and operand.</summary>
		/// <returns>A copy of this CodeInstruction.</returns>
		///
		public CodeInstruction Clone()
		{
			return new CodeInstruction(this)
			{
				labels = new List<Label>(),
				blocks = new List<ExceptionBlock>()
			};
		}

		/// <summary>copy only operand, use new opcode.</summary>
		/// <param name="opcode">The opcode.</param>
		/// <returns>A copy of this CodeInstruction.</returns>
		///
		public CodeInstruction Clone(OpCode opcode)
		{
			var instruction = Clone();
			instruction.opcode = opcode;
			return instruction;
		}

		/// <summary>copy only opcode, use new operand.</summary>
		/// <param name="operand">The operand.</param>
		/// <returns>A copy of this CodeInstruction.</returns>
		///
		public CodeInstruction Clone(object operand)
		{
			var instruction = Clone();
			instruction.operand = operand;
			return instruction;
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		///
		public override string ToString()
		{
			var list = new List<string>();
			foreach (var label in labels)
				list.Add("Label" + label.GetHashCode());
			foreach (var block in blocks)
				list.Add("EX_" + block.blockType.ToString().Replace("Block", ""));

			var extras = list.Count > 0 ? " [" + string.Join(", ", list.ToArray()) + "]" : "";
			var operandStr = Emitter.FormatArgument(operand);
			if (operandStr != "") operandStr = " " + operandStr;
			return opcode + operandStr + extras;
		}
	}
}