﻿using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void TypeBehaviorAction(Type fixtureClass, Convention convention, Case[] cases, TypeBehavior inner);
    public delegate ExceptionList TypeAction(Type fixtureClass);

    public class TypeBehaviorBuilder
    {
        public TypeBehaviorBuilder()
        {
            Behavior = null;
        }

        public TypeBehavior Behavior { get; private set; }

        public TypeBehaviorBuilder CreateInstancePerCase()
        {
            Behavior = new CreateInstancePerCase();
            return this;
        }

        public TypeBehaviorBuilder CreateInstancePerFixture()
        {
            Behavior = new CreateInstancePerFixture();
            return this;
        }

        public TypeBehaviorBuilder Wrap(TypeBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public TypeBehaviorBuilder SetUpTearDown(TypeAction setUp, TypeAction tearDown)
        {
            return Wrap((fixtureClass, convention, cases, inner) =>
            {
                var setUpExceptions = setUp(fixtureClass);
                if (setUpExceptions.Any())
                {
                    foreach (var @case in cases)
                        @case.Exceptions.Add(setUpExceptions);
                    return;
                }

                inner.Execute(fixtureClass, convention, cases);

                var tearDownExceptions = tearDown(fixtureClass);
                if (tearDownExceptions.Any())
                    foreach (var @case in cases)
                        @case.Exceptions.Add(tearDownExceptions);
            });
        }

        class WrapBehavior : TypeBehavior
        {
            readonly TypeBehaviorAction outer;
            readonly TypeBehavior inner;

            public WrapBehavior(TypeBehaviorAction outer, TypeBehavior inner)
            {
                this.outer = outer;
                this.inner = inner;
            }

            public void Execute(Type fixtureClass, Convention convention, Case[] cases)
            {
                try
                {
                    outer(fixtureClass, convention, cases, inner);
                }
                catch (Exception exception)
                {
                    foreach (var @case in cases)
                    {
                        @case.Exceptions.Add(exception);
                    }
                }                
            }
        }
    }
}