﻿using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public class IfBlock : CompositeFrame
    {
        public string Condition { get; }

        public IfBlock(string condition, params Frame[] inner) : base(inner)
        {
            Condition = condition;
        }

        public IfBlock(Variable variable, params Frame[] inner) : this(variable.Usage, inner)
        {
            
        }

        protected override void generateCode(GeneratedMethod method, ISourceWriter writer, Frame inner)
        {
            writer.WriteLine($"BLOCK:if ({Condition})");
            inner.GenerateCode(method, writer);
            writer.FinishBlock();
        }
    }
}