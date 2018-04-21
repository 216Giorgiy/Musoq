﻿using System;

namespace Musoq.Parser.Nodes
{
    public class DotNode : UnaryNode
    {
        public Node Root { get; }

        public bool IsOuter { get; }

        public string Name { get; }

        public DotNode(Node root, Node expression, bool isOuter, string name)
            : base(expression)
        {
            Root = root;
            IsOuter = isOuter;
            Name = name;
            Id = $"{nameof(DotNode)}{root.ToString()}{expression.ToString()}{isOuter}{name}";
        }

        public override Type ReturnType => Expression.ReturnType;

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string Id { get; }

        public override string ToString()
        {
            return $"{Root.ToString()}.{Expression.ToString()}";
        }
    }
}
