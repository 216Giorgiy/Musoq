﻿using System;

namespace Musoq.Parser.Nodes
{
    public class DotNode : UnaryNode
    {
        public DotNode(Node root, Node expression, bool isOuter, string name, Type returnType = null)
            : base(expression)
        {
            Root = root;
            IsOuter = isOuter;
            Name = name;
            Id = $"{nameof(DotNode)}{root.ToString()}{expression.ToString()}{isOuter}{name}";
            ReturnType = returnType;
        }

        public Node Root { get; }

        public bool IsOuter { get; }

        public string Name { get; }

        public override Type ReturnType { get; }

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{Root.ToString()}.{Expression.ToString()}";
        }
    }
}