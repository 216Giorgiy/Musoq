﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Musoq.Converter;
using Musoq.Evaluator.Instructions;
using Musoq.Plugins;
using Musoq.Plugins.Attributes;
using Musoq.Schema;
using Musoq.Schema.DataSources;
using Musoq.Schema.Managers;

namespace Musoq.Evaluator.Tests
{
    [TestClass]
    public class BasicEvaluatorTests
    {
        [TestMethod]
        public void LikeOperator()
        {
            var query = "select Name from #A.Entities() where Name like '%AA%'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("ABCAACBA"), new BasicEntity("AAeqwgQEW"), new BasicEntity("XXX"), new BasicEntity("dadsqqAA")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("ABCAACBA", table[0].Values[0]);
            Assert.AreEqual("AAeqwgQEW", table[1].Values[0]);
            Assert.AreEqual("dadsqqAA", table[2].Values[0]);
        }

        [TestMethod]
        public void NotLikeOperator()
        {
            var query = "select Name from #A.Entities() where Name not like '%AA%'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("ABCAACBA"), new BasicEntity("AAeqwgQEW"), new BasicEntity("XXX"), new BasicEntity("dadsqqAA")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("XXX", table[0].Values[0]);
        }

        [TestMethod]
        public void AddOperatorWithStringsTurnsIntoConcat()
        {
            var query = "select 'abc' + 'cda' from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("ABCAACBA")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("'abc' + 'cda'", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("abccda", table[0].Values[0]);
        }

        [TestMethod]
        public void ContainsStrings()
        {
            var query = "select Name from #A.Entities() where Name contains ('ABC', 'CdA', 'CDA')";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("ABC"), new BasicEntity("XXX"), new BasicEntity("CDA")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("ABC", table[0].Values[0]);
            Assert.AreEqual("CDA", table[1].Values[0]);
        }

        [TestMethod]
        public void CanPassComplexArgumentToFunction()
        {
            var query = "select NothingToDo(Self) from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"){ Name = "ABBA", Country = "POLAND", City = "CRACOV", Money = 1.23m, Month = "JANUARY", Population = 250}}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("NothingToDo(Self)", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(typeof(BasicEntity), table[0].Values[0].GetType());
        }

        [TestMethod]
        public void TableShouldComplexType()
        {
            var query = "select Self from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"){ Name = "ABBA", Country = "POLAND", City = "CRACOV", Money = 1.23m, Month = "JANUARY", Population = 250}}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Self", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(typeof(BasicEntity), table[0].Values[0].GetType());
        }

        [TestMethod]
        public void SimpleShowAllColumns()
        {
            var entity = new BasicEntity("001")
            {
                Name = "ABBA",
                Country = "POLAND",
                City = "CRACOV",
                Money = 1.23m,
                Month = "JANUARY",
                Population = 250,
                Time = DateTime.MaxValue
            };
            var query = "select 1, *, Name as Name2, ToString(Self) as SelfString from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {entity}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();
            Assert.AreEqual("1", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(Int64), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("Name", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("City", table.Columns.ElementAt(2).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(2).ColumnType);

            Assert.AreEqual("Country", table.Columns.ElementAt(3).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual("Population", table.Columns.ElementAt(4).Name);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(4).ColumnType);

            Assert.AreEqual("Self", table.Columns.ElementAt(5).Name);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(5).ColumnType);

            Assert.AreEqual("Money", table.Columns.ElementAt(6).Name);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(6).ColumnType);

            Assert.AreEqual("Month", table.Columns.ElementAt(7).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(7).ColumnType);

            Assert.AreEqual("Time", table.Columns.ElementAt(8).Name);
            Assert.AreEqual(typeof(DateTime), table.Columns.ElementAt(8).ColumnType);

            Assert.AreEqual("Name2", table.Columns.ElementAt(9).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(9).ColumnType);

            Assert.AreEqual("SelfString", table.Columns.ElementAt(10).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(10).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(Convert.ToInt64(1), table[0].Values[0]);
            Assert.AreEqual("ABBA", table[0].Values[1]);
            Assert.AreEqual("CRACOV", table[0].Values[2]);
            Assert.AreEqual("POLAND", table[0].Values[3]);
            Assert.AreEqual(250m, table[0].Values[4]);
            Assert.AreEqual(entity, table[0].Values[5]);
            Assert.AreEqual(1.23m, table[0].Values[6]);
            Assert.AreEqual("JANUARY", table[0].Values[7]);
            Assert.AreEqual(DateTime.MaxValue, table[0].Values[8]);
            Assert.AreEqual("ABBA", table[0].Values[9]);
            Assert.AreEqual("TEST STRING", table[0].Values[10]);
        }

        [TestMethod]
        public void SimpleAccessObjectTest()
        {
            var query = @"select Self.Array[2] from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual("Self.Array[2]", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(Int32), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(2, table[0].Values[0]);
            Assert.AreEqual(2, table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleAccessObjectIncrementTest()
        {
            var query = @"select Inc(Self.Array[2]) from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual("Inc(Self.Array[2])", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(Int64), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(Convert.ToInt64(3), table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt64(3), table[1].Values[0]);
        }

        [TestMethod]
        public void WhereWithAndTest()
        {
            var query = @"select Name from #A.Entities() where Name = '001' or Name = '005'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"),  new BasicEntity("002"), new BasicEntity("005")}},
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("005", table[1].Values[0]);
        }

        [TestMethod]
        public void GroupByWithParentSum()
        {
            var query = @"select SumIncome(1, Money), SumOutcome(1, Money) from #A.Entities() group by Month, City";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[]
                {
                    new BasicEntity("czestochowa", "jan", Convert.ToDecimal(400)),
                    new BasicEntity("katowice", "jan", Convert.ToDecimal(300)),
                    new BasicEntity("cracow", "jan", Convert.ToDecimal(-200))
                }}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual(Convert.ToDecimal(700), table[0].Values[0]);
            Assert.AreEqual(Convert.ToDecimal(-200), table[0].Values[1]);
            Assert.AreEqual(Convert.ToDecimal(700), table[1].Values[0]);
            Assert.AreEqual(Convert.ToDecimal(-200), table[1].Values[1]);
            Assert.AreEqual(Convert.ToDecimal(700), table[2].Values[0]);
            Assert.AreEqual(Convert.ToDecimal(-200), table[2].Values[1]);
        }

        [TestMethod]
        public void GroupBySubtractGroupsTest()
        {
            var query = @"select SumIncome(Money), SumOutcome(Money), SumIncome(Money) - Abs(SumOutcome(Money)) from #A.Entities() group by Month";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("jan", Convert.ToDecimal(400)), new BasicEntity("jan", Convert.ToDecimal(300)), new BasicEntity("jan", Convert.ToDecimal(-200)) }}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(Convert.ToDecimal(700), table[0].Values[0]);
            Assert.AreEqual(Convert.ToDecimal(-200), table[0].Values[1]);
            Assert.AreEqual(Convert.ToDecimal(500), table[0].Values[2]);
        }

        [TestMethod]
        public void SimpleQueryTest()
        {
            var query = @"select Name from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipTest()
        {
            var query = @"select Name from #A.Entities() skip 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("005"), new BasicEntity("006")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Count);
            Assert.AreEqual("003", table[0].Values[0]);
            Assert.AreEqual("004", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
            Assert.AreEqual("006", table[3].Values[0]);
        }

        [TestMethod]
        public void SimpleTakeTest()
        {
            var query = @"select Name from #A.Entities() take 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("005"), new BasicEntity("006")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipTakeTest()
        {
            var query = @"select Name from #A.Entities() skip 1 take 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("005"), new BasicEntity("006")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("002", table[0].Values[0]);
            Assert.AreEqual("003", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() skip 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("005"), new BasicEntity("006")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void SimpleTakeAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() take 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("005"), new BasicEntity("006")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("003", table[2].Values[0]);
            Assert.AreEqual("004", table[3].Values[0]);
            Assert.AreEqual("005", table[4].Values[0]);
            Assert.AreEqual("006", table[5].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipTakeAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() skip 100 take 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("005"), new BasicEntity("006")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void UnionWithDifferentColumnsAsAKey()
        {
            var query = @"select Name from #A.Entities() union (Name) select MyName as Name from #B.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("003", table[2].Values[0]);
            Assert.AreEqual("004", table[3].Values[0]);
        }

        [TestMethod]
        public void UnionWithoutDuplicatedKeysTest()
        {
            var query = @"select Name from #A.Entities() union (Name) select Name from #B.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("003", table[2].Values[0]);
            Assert.AreEqual("004", table[3].Values[0]);
        }

        [TestMethod]
        public void UnionWithDuplicatedKeysTest()
        {
            var query = @"select Name from #A.Entities() union (Name) select Name from #B.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("005")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
        }

        [TestMethod]
        public void MultipleUnionsWithDuplicatedKeysTest()
        {
            var query =
                @"select Name from #A.Entities() union (Name) select Name from #B.Entities() union (Name) select Name from #C.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}},
                {"#B", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#C", new[] {new BasicEntity("005")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
        }

        [TestMethod]
        public void MultipleUnionsWithoutDuplicatedKeysTest()
        {
            var query =
                @"select Name from #A.Entities() union (Name) select Name from #B.Entities() union (Name) select Name from #C.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}},
                {"#B", new[] {new BasicEntity("002")}},
                {"#C", new[] {new BasicEntity("005")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
        }

        [TestMethod]
        public void MultipleUnionsComplexTest()
        {
            var query =
                @"select Name from #A.Entities() union (Name) select Name from #B.Entities() union (Name) select Name from #C.Entities() union (Name) select Name from #D.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}},
                {"#B", new[] {new BasicEntity("002")}},
                {"#C", new[] {new BasicEntity("005")}},
                {"#D", new[] {new BasicEntity("007"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
            Assert.AreEqual("007", table[3].Values[0]);
        }

        [TestMethod]
        public void UnionAllWithDuplicatedKeysTest()
        {
            var query = @"select Name from #A.Entities() union all (Name) select Name from #B.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("005")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(5, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("001", table[2].Values[0]);
            Assert.AreEqual("002", table[3].Values[0]);
            Assert.AreEqual("005", table[4].Values[0]);
        }

        [TestMethod]
        public void MultipleUnionsAllWithDuplicatedKeysTest()
        {
            var query =
                @"select Name from #A.Entities() union all (Name) select Name from #B.Entities() union all (Name) select Name from #C.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}},
                {"#B", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#C", new[] {new BasicEntity("005")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("001", table[1].Values[0]);
            Assert.AreEqual("002", table[2].Values[0]);
            Assert.AreEqual("005", table[3].Values[0]);
        }

        [TestMethod]
        public void MultipleUnionsAllWithoutDuplicatedKeysTest()
        {
            var query =
                @"select Name from #A.Entities() union all (Name) select Name from #B.Entities() union all (Name) select Name from #C.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}},
                {"#B", new[] {new BasicEntity("002")}},
                {"#C", new[] {new BasicEntity("005")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
        }

        [TestMethod]
        public void MultipleUnionsAllComplexTest()
        {
            var query =
                @"select Name from #A.Entities() union all (Name) select Name from #B.Entities() union all (Name) select Name from #C.Entities() union all (Name) select Name from #D.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}},
                {"#B", new[] {new BasicEntity("002")}},
                {"#C", new[] {new BasicEntity("005")}},
                {"#D", new[] {new BasicEntity("007"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(5, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
            Assert.AreEqual("007", table[3].Values[0]);
            Assert.AreEqual("001", table[4].Values[0]);
        }

        [TestMethod]
        public void UnionAllWithoutDuplicatedKeysTest()
        {
            var query = @"select Name from #A.Entities() union all (Name) select Name from #B.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(5, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("003", table[2].Values[0]);
            Assert.AreEqual("004", table[3].Values[0]);
            Assert.AreEqual("001", table[4].Values[0]);
        }

        [TestMethod]
        public void ExceptDoubleSourceTest()
        {
            var query = @"select Name from #A.Entities() except (Name) select Name from #B.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("002", table[0].Values[0]);
        }

        [TestMethod]
        public void ExceptTripleSourcesTest()
        {
            var query =
                @"select Name from #A.Entities() except (Name) select Name from #B.Entities() except (Name) select Name from #C.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("001")}},
                {"#C", new[] {new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void IntersectDoubleSourceTest()
        {
            var query = @"select Name from #A.Entities() intersect (Name) select Name from #B.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
        }

        [TestMethod]
        public void IntersectTripleSourcesTest()
        {
            var query =
                @"select Name from #A.Entities() intersect (Name) select Name from #B.Entities() intersect (Name) select Name from #C.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("001")}},
                {"#C", new[] {new BasicEntity("002"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
        }

        [TestMethod]
        public void MixedSourcesExceptUnionScenarioTest()
        {
            var query =
                @"select Name from #A.Entities()
except (Name)
select Name from #B.Entities()
union (Name)
select Name from #C.Entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("001")}},
                {"#C", new[] {new BasicEntity("002"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("002", table[0].Values[0]);
            Assert.AreEqual("001", table[1].Values[0]);
        }

        [TestMethod]
        public void MixedSourcesExceptUnionWithConditionsScenarioTest()
        {
            var query =
                @"select Name from #A.Entities() where Extension = '.txt'
except (Name)
select Name from #B.Entities() where Extension = '.txt'
union (Name)
select Name from #C.Entities() where Extension = '.txt'";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("001")}},
                {"#C", new[] {new BasicEntity("002"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("002", table[0].Values[0]);
            Assert.AreEqual("001", table[1].Values[0]);
        }

        [TestMethod]
        public void MixedSourcesExceptUnionWithMultipleColumnsConditionsScenarioTest()
        {
            var query =
                @"select Name, RandomNumber() from #A.Entities() where Extension = '.txt'
except (Name)
select Name, RandomNumber() from #B.Entities() where Extension = '.txt'
union (Name)
select Name, RandomNumber() from #C.Entities() where Extension = '.txt'";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}},
                {"#B", new[] {new BasicEntity("003"), new BasicEntity("004"), new BasicEntity("001")}},
                {"#C", new[] {new BasicEntity("002"), new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("002", table[0].Values[0]);
            Assert.AreEqual("001", table[1].Values[0]);
        }

        [TestMethod]
        public void ColumnNamesSimpleTest()
        {
            var query = @"select Name, GetOne(), GetOne() as TestColumn, GetTwo(4d, 'test') from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new BasicEntity[] { }}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("GetOne()", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("TestColumn", table.Columns.ElementAt(2).Name);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);

            Assert.AreEqual("GetTwo(4, 'test')", table.Columns.ElementAt(3).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(3).ColumnType);
        }

        [TestMethod]
        public void CallMethodWithTwoParameters()
        {
            var query = @"select Concat(Country, ToString(Population)) from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("ABBA", 200)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Concat(Country, ToString(Population))", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("ABBA200", table[0].Values[0]);
        }

        [TestMethod]
        public void SimpleGroupByTest()
        {
            var query = @"select Name, Count(Name) from #A.Entities() group by Name having Count(Name) >= 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("ABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("CECCA"),
                        new BasicEntity("ABBA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("Count(Name)", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("ABBA", table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt32(4), table[0].Values[1]);
            Assert.AreEqual("BABBA", table[1].Values[0]);
            Assert.AreEqual(Convert.ToInt32(2), table[1].Values[1]);
        }

        [TestMethod]
        public void SimpleGroupByWithSkipTest()
        {
            var query = @"select Name, Count(Name) from #A.Entities() group by Name skip 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("ABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("CECCA"),
                        new BasicEntity("ABBA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("Count(Name)", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("CECCA", table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt32(1), table[0].Values[1]);
        }

        [TestMethod]
        public void SimpleGroupByWithTakeTest()
        {
            var query = @"select Name, Count(Name) from #A.Entities() group by Name take 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("ABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("CECCA"),
                        new BasicEntity("ABBA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("Count(Name)", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("ABBA", table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt32(4), table[0].Values[1]);
            Assert.AreEqual("BABBA", table[1].Values[0]);
            Assert.AreEqual(Convert.ToInt32(2), table[1].Values[1]);
        }

        [TestMethod]
        public void SimpleGroupByWithSkipTakeTest()
        {
            var query = @"select Name, Count(Name) from #A.Entities() group by Name skip 2 take 1";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("ABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("CECCA"),
                        new BasicEntity("ABBA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("Count(Name)", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("CECCA", table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt32(1), table[0].Values[1]);
        }

        [TestMethod]
        public void GroupByWithValueTest()
        {
            var query = @"select Country, Sum(Population) from #A.Entities() group by Country";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("ABBA", 200),
                        new BasicEntity("ABBA", 500),
                        new BasicEntity("BABBA", 100),
                        new BasicEntity("ABBA", 10),
                        new BasicEntity("BABBA", 100),
                        new BasicEntity("CECCA", 1000)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("Country", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("Sum(Population)", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("ABBA", table[0].Values[0]);
            Assert.AreEqual(Convert.ToDecimal(710), table[0].Values[1]);
            Assert.AreEqual("BABBA", table[1].Values[0]);
            Assert.AreEqual(Convert.ToDecimal(200), table[1].Values[1]);
            Assert.AreEqual("CECCA", table[2].Values[0]);
            Assert.AreEqual(Convert.ToDecimal(1000), table[2].Values[1]);
        }

        [TestMethod]
        public void GroupByMultipleColumnsTest()
        {
            var query = @"select Country, City, Count(Country), Count(City) from #A.Entities() group by Country, City";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("POLAND", "WARSAW"),
                        new BasicEntity("POLAND", "CZESTOCHOWA"),
                        new BasicEntity("UK", "LONDON"),
                        new BasicEntity("POLAND", "CZESTOCHOWA"),
                        new BasicEntity("UK", "MANCHESTER"),
                        new BasicEntity("ANGOLA", "LLL")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Columns.Count());
            Assert.AreEqual("Country", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("City", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual("Count(Country)", table.Columns.ElementAt(2).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual("Count(City)", table.Columns.ElementAt(3).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual(5, table.Count);
            Assert.AreEqual("POLAND", table[0].Values[0]);
            Assert.AreEqual("WARSAW", table[0].Values[1]);
            Assert.AreEqual(Convert.ToInt32(1), table[0].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[0].Values[3]);
            Assert.AreEqual("POLAND", table[1].Values[0]);
            Assert.AreEqual("CZESTOCHOWA", table[1].Values[1]);
            Assert.AreEqual(Convert.ToInt32(2), table[1].Values[2]);
            Assert.AreEqual(Convert.ToInt32(2), table[1].Values[3]);
            Assert.AreEqual("UK", table[2].Values[0]);
            Assert.AreEqual("LONDON", table[2].Values[1]);
            Assert.AreEqual(Convert.ToInt32(1), table[2].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[2].Values[3]);
            Assert.AreEqual("UK", table[3].Values[0]);
            Assert.AreEqual("MANCHESTER", table[3].Values[1]);
            Assert.AreEqual(Convert.ToInt32(1), table[3].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[3].Values[3]);
            Assert.AreEqual("ANGOLA", table[4].Values[0]);
            Assert.AreEqual("LLL", table[4].Values[1]);
            Assert.AreEqual(Convert.ToInt32(1), table[4].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[4].Values[3]);
        }

        [TestMethod]
        public void GroupBySubstr()
        {
            var query =
                @"select Substr(Name, 0, 2), Count(Substr(Name, 0, 2)) from #A.Entities() group by Substr(Name, 0, 2)";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("AA:1"),
                        new BasicEntity("AA:2"),
                        new BasicEntity("AA:3"),
                        new BasicEntity("BB:1"),
                        new BasicEntity("BB:2"),
                        new BasicEntity("CC:1")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("Substr(Name, 0, 2)", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("Count(Substr(Name, 0, 2))", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(3, table.Count);

            Assert.AreEqual("AA", table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt32(3), table[0].Values[1]);

            Assert.AreEqual("BB", table[1].Values[0]);
            Assert.AreEqual(Convert.ToInt32(2), table[1].Values[1]);

            Assert.AreEqual("CC", table[2].Values[0]);
            Assert.AreEqual(Convert.ToInt32(1), table[2].Values[1]);
        }

        [TestMethod]
        public void GroupByWithSelectedConstantModifiedByFunction()
        {
            var query =
                @"select Name, Count(Name), Inc(10d), 1 from #A.Entities() group by Name having Count(Name) >= 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("ABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("ABBA"),
                        new BasicEntity("BABBA"),
                        new BasicEntity("CECCA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("Count(Name)", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual("Inc(10)", table.Columns.ElementAt(2).Name);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual("1", table.Columns.ElementAt(3).Name);
            Assert.AreEqual(typeof(long), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("ABBA", table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt32(3), table[0].Values[1]);
            Assert.AreEqual(Convert.ToDecimal(11), table[0].Values[2]);
            Assert.AreEqual(Convert.ToInt64(1), table[0].Values[3]);
            Assert.AreEqual("BABBA", table[1].Values[0]);
            Assert.AreEqual(Convert.ToInt32(2), table[1].Values[1]);
            Assert.AreEqual(Convert.ToDecimal(11), table[1].Values[2]);
            Assert.AreEqual(Convert.ToInt64(1), table[1].Values[3]);
        }

        [TestMethod]
        public void GroupByColumnSubstring()
        {
            var query = "select Country, Substr(City, IndexOf(City, ':')) as 'City', Count(City) as 'Count', Sum(Population) as 'Sum' from #A.Entities() group by Substr(City, IndexOf(City, ':')), Country";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW:TARGOWEK", "POLAND", 500),
                        new BasicEntity("WARSAW:URSYNOW", "POLAND", 500),
                        new BasicEntity("KATOWICE:ZAWODZIE", "POLAND", 250)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Columns.Count());
            Assert.AreEqual("Country", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("City", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual("Count", table.Columns.ElementAt(2).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual("Sum", table.Columns.ElementAt(3).Name);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("POLAND", table[0].Values[0]);
            Assert.AreEqual("WARSAW", table[0].Values[1]);
            Assert.AreEqual(Convert.ToInt32(2), table[0].Values[2]);
            Assert.AreEqual(Convert.ToDecimal(1000), table[0].Values[3]);
            Assert.AreEqual("POLAND", table[1].Values[0]);
            Assert.AreEqual("KATOWICE", table[1].Values[1]);
            Assert.AreEqual(Convert.ToInt32(1), table[1].Values[2]);
            Assert.AreEqual(Convert.ToDecimal(250), table[1].Values[3]);
        }

        [TestMethod]
        public void GroupByWithParentCount()
        {
            var query = "select Country, City as 'City', ParentCount(1), Count(City) as 'CountOfCities' from #A.Entities() group by Country, City";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Columns.Count());
            Assert.AreEqual("Country", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("City", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual("ParentCount(1)", table.Columns.ElementAt(2).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual("CountOfCities", table.Columns.ElementAt(3).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual(5, table.Count);
            Assert.AreEqual("POLAND", table[0].Values[0]);
            Assert.AreEqual("WARSAW", table[0].Values[1]);
            Assert.AreEqual(Convert.ToInt32(3), table[0].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[0].Values[3]);

            Assert.AreEqual("POLAND", table[1].Values[0]);
            Assert.AreEqual("CZESTOCHOWA", table[1].Values[1]);
            Assert.AreEqual(Convert.ToInt32(3), table[1].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[1].Values[3]);

            Assert.AreEqual("POLAND", table[2].Values[0]);
            Assert.AreEqual("KATOWICE", table[2].Values[1]);
            Assert.AreEqual(Convert.ToInt32(3), table[2].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[2].Values[3]);

            Assert.AreEqual("GERMANY", table[3].Values[0]);
            Assert.AreEqual("BERLIN", table[3].Values[1]);
            Assert.AreEqual(Convert.ToInt32(2), table[3].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[3].Values[3]);

            Assert.AreEqual("GERMANY", table[4].Values[0]);
            Assert.AreEqual("MUNICH", table[4].Values[1]);
            Assert.AreEqual(Convert.ToInt32(2), table[4].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[4].Values[3]);
        }

        [TestMethod]
        public void GroupByWithWhere()
        {
            var query = "select Country, City as 'City', ParentCount(1), Count(City) as 'CountOfCities' from #A.Entities() where Country = 'POLAND' group by Country, City";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(4, table.Columns.Count());
            Assert.AreEqual("Country", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("City", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual("ParentCount(1)", table.Columns.ElementAt(2).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual("CountOfCities", table.Columns.ElementAt(3).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("POLAND", table[0].Values[0]);
            Assert.AreEqual("WARSAW", table[0].Values[1]);
            Assert.AreEqual(Convert.ToInt32(3), table[0].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[0].Values[3]);

            Assert.AreEqual("POLAND", table[1].Values[0]);
            Assert.AreEqual("CZESTOCHOWA", table[1].Values[1]);
            Assert.AreEqual(Convert.ToInt32(3), table[1].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[1].Values[3]);

            Assert.AreEqual("POLAND", table[2].Values[0]);
            Assert.AreEqual("KATOWICE", table[2].Values[1]);
            Assert.AreEqual(Convert.ToInt32(3), table[2].Values[2]);
            Assert.AreEqual(Convert.ToInt32(1), table[2].Values[3]);
        }

        [TestMethod]
        public void GroupWasNotListed()
        {
            var query = "select Count(Country) from #A.entities() group by Country";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Count(Country)", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(3, table[0].Values[0]);
            Assert.AreEqual(2, table[1].Values[0]);
        }

        [TestMethod]
        public void CountWithFakeGroupBy()
        {
            var query = "select Count(Country) from #A.entities() group by 'fake'";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Count(Country)", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(5, table[0].Values[0]);
        }

        [TestMethod]
        public void CountWithoutGroupBy()
        {
            var query = "select Count(Country), Sum(Population) from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("Count(Country)", table.Columns.ElementAt(0).Name);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("Sum(Population)", table.Columns.ElementAt(1).Name);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(5, table[0].Values[0]);
            Assert.AreEqual(Convert.ToDecimal(1750), table[0].Values[1]);
        }

        [TestMethod]
        public void ArithmeticOpsGreater()
        {
            var query = "select City from #A.entities() where Population > 400d";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("City", table.Columns.ElementAt(0).Name);

            Assert.AreEqual(1, table.Count());
            Assert.AreEqual("WARSAW", table[0].Values[0]);
        }

        [TestMethod]
        public void ArithmeticOpsGreaterEqual()
        {
            var query = "select City from #A.entities() where Population >= 400d";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("City", table.Columns.ElementAt(0).Name);

            Assert.AreEqual(2, table.Count());
            Assert.AreEqual("WARSAW", table[0].Values[0]);
            Assert.AreEqual("CZESTOCHOWA", table[1].Values[0]);
        }

        [TestMethod]
        public void ArithmeticOpsEquals()
        {
            var query = "select City from #A.entities() where Population = 250d";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("City", table.Columns.ElementAt(0).Name);

            Assert.AreEqual(2, table.Count());
            Assert.AreEqual("KATOWICE", table[0].Values[0]);
            Assert.AreEqual("BERLIN", table[1].Values[0]);
        }

        [TestMethod]
        public void ArithmeticOpsLess()
        {
            var query = "select City from #A.entities() where Population < 350d";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("City", table.Columns.ElementAt(0).Name);

            Assert.AreEqual(2, table.Count());
            Assert.AreEqual("KATOWICE", table[0].Values[0]);
            Assert.AreEqual("BERLIN", table[1].Values[0]);
        }


        [TestMethod]
        public void ArithmeticOpsLessEqual()
        {
            var query = "select City from #A.entities() where Population <= 350d";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 400),
                        new BasicEntity("KATOWICE", "POLAND", 250),
                        new BasicEntity("BERLIN", "GERMANY", 250),
                        new BasicEntity("MUNICH", "GERMANY", 350)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("City", table.Columns.ElementAt(0).Name);

            Assert.AreEqual(3, table.Count());
            Assert.AreEqual("KATOWICE", table[0].Values[0]);
            Assert.AreEqual("BERLIN", table[1].Values[0]);
            Assert.AreEqual("MUNICH", table[2].Values[0]);
        }

        [TestMethod]
        public void ColumnTypeDateTime()
        {
            var query = "select Time from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity(DateTime.MinValue)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Execute();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Time", table.Columns.ElementAt(0).Name);

            Assert.AreEqual(1, table.Count());
            Assert.AreEqual(DateTime.MinValue, table[0].Values[0]);
        }

        private IVirtualMachine CreateAndRunVirtualMachine<T>(string script,
            IDictionary<string, IEnumerable<T>> sources)
            where T : BasicEntity
        {
            return InstanceCreator.Create(script, new SchemaProvider<T>(sources));
        }

        private class BasicEntity
        {
            public BasicEntity(string name)
            {
                Name = name;
            }

            public BasicEntity(string country, string city)
            {
                Country = country;
                City = city;
            }

            public BasicEntity(string country, int population)
            {
                Country = country;
                Population = population;
            }

            public BasicEntity(string city, string country, int population)
            {
                City = city;
                Country = country;
                Population = population;
            }

            public BasicEntity(string month, decimal money)
            {
                Month = month;
                Money = money;
            }

            public BasicEntity(string city, string month, decimal money)
            {
                City = city;
                Month = month;
                Money = money;
            }

            public BasicEntity(DateTime time)
            {
                Time = time;
            }

            public string Month { get; set; }
            public string Name { get; set; }
            public string Country { get; set; }

            public string City { get; set; }
            public decimal Population { get; set; }
            public decimal Money { get; set; }
            public DateTime Time { get; set; }

            public BasicEntity Self => this;

            public int[] Array => new [] {0, 1, 2};

            public override string ToString()
            {
                return "TEST STRING";
            }
        }

        private class SchemaColumn : ISchemaColumn
        {
            public SchemaColumn(string columnName, int columnIndex, Type columnType)
            {
                ColumnName = columnName;
                ColumnIndex = columnIndex;
                ColumnType = columnType;
            }

            public string ColumnName { get; }
            public int ColumnIndex { get; }
            public Type ColumnType { get; }
        }

        private class TestLibrary : LibraryBase
        {
            private readonly Random _random = new Random();

            [BindableMethod]
            public string Name([InjectSource] BasicEntity entity)
            {
                return entity.Name;
            }

            [BindableMethod]
            public string MyName([InjectSource] BasicEntity entity)
            {
                return entity.Name;
            }

            [BindableMethod]
            public string Extension([InjectSource] BasicEntity entity)
            {
                return ".txt";
            }

            [BindableMethod]
            public int RandomNumber()
            {
                return _random.Next(0, 100);
            }

            [BindableMethod]
            public decimal GetOne()
            {
                return 1;
            }

            [BindableMethod]
            public string GetTwo(decimal a, string b)
            {
                return 2.ToString();
            }

            [BindableMethod]
            public decimal Inc(decimal number)
            {
                return number + 1;
            }

            [BindableMethod]
            public long Inc(long number)
            {
                return number + 1;
            }

            [BindableMethod]
            public BasicEntity NothingToDo(BasicEntity entity)
            {
                return entity;
            }
        }

        private class TestSchema<T> : ISchema
            where T : BasicEntity
        {
            private readonly MethodsAggregator _aggreagator;
            private readonly IEnumerable<T> _sources;

            public TestSchema(IEnumerable<T> sources)
            {
                _sources = sources;

                var methodManager = new MethodsManager();
                var propertiesManager = new PropertiesManager();

                var lib = new TestLibrary();

                propertiesManager.RegisterProperties(lib);
                methodManager.RegisterLibraries(lib);

                _aggreagator = new MethodsAggregator(methodManager, propertiesManager);
            }

            public string Name => "test";

            public ISchemaTable GetTableByName(string name, string[] parameters)
            {
                return new BasicEntitySchema();
            }

            public RowSource GetRowSource(string name, string[] parameters)
            {
                return new EntitySource<T>(_sources,
                    new Dictionary<string, int>
                    {
                        {nameof(BasicEntity.Name), 10},
                        {nameof(BasicEntity.City), 11},
                        {nameof(BasicEntity.Country), 12},
                        {nameof(BasicEntity.Population), 13},
                        {nameof(BasicEntity.Self), 14},
                        {nameof(BasicEntity.Money), 15},
                        {nameof(BasicEntity.Month), 16},
                        {nameof(BasicEntity.Time), 17}
                    },
                    new Dictionary<int, Func<T, object>>
                    {
                        {10, arg => arg.Name},
                        {11, arg => arg.City},
                        {12, arg => arg.Country},
                        {13, arg => arg.Population},
                        {14, arg => arg.Self},
                        {15, arg => arg.Money},
                        {16, arg => arg.Month},
                        {17, arg => arg.Time}
                    });
            }

            public MethodInfo ResolveMethod(string method, Type[] parameters)
            {
                return _aggreagator.ResolveMethod(method, parameters);
            }

            public MethodInfo ResolveAggregationMethod(string method, Type[] parameters)
            {
                return _aggreagator.ResolveMethod(method, parameters);
            }

            public bool TryResolveAggreationMethod(string method, Type[] parameters, out MethodInfo methodInfo)
            {
                var founded = _aggreagator.TryResolveMethod(method, parameters, out methodInfo);
                if (founded)
                    return methodInfo.GetCustomAttribute<AggregationMethodAttribute>() != null;

                return false;
            }

            public MethodInfo ResolveProperty(string property)
            {
                return _aggreagator.ResolveProperty(property);
            }

            private class BasicEntitySchema : ISchemaTable
            {
                public BasicEntitySchema()
                {
                    Columns = new ISchemaColumn[]
                    {
                        new SchemaColumn(nameof(BasicEntity.Name), 10,
                            typeof(BasicEntity).GetProperty(nameof(BasicEntity.Name)).PropertyType),
                        new SchemaColumn(nameof(BasicEntity.City), 11,
                            typeof(BasicEntity).GetProperty(nameof(BasicEntity.City)).PropertyType),
                        new SchemaColumn(nameof(BasicEntity.Country), 12,
                            typeof(BasicEntity).GetProperty(nameof(BasicEntity.Country)).PropertyType),
                        new SchemaColumn(nameof(BasicEntity.Population), 13,
                            typeof(BasicEntity).GetProperty(nameof(BasicEntity.Population)).PropertyType),
                        new SchemaColumn(nameof(BasicEntity.Self), 14, 
                            typeof(BasicEntity).GetProperty(nameof(BasicEntity.Self)).PropertyType),
                        new SchemaColumn(nameof(BasicEntity.Money), 15,
                            typeof(BasicEntity).GetProperty(nameof(BasicEntity.Money)).PropertyType),
                        new SchemaColumn(nameof(BasicEntity.Month), 16,
                            typeof(BasicEntity).GetProperty(nameof(BasicEntity.Month)).PropertyType),
                        new SchemaColumn(nameof(BasicEntity.Time), 17,
                            typeof(BasicEntity).GetProperty(nameof(BasicEntity.Time)).PropertyType)
                    };
                }

                public ISchemaColumn[] Columns { get; }
            }
        }

        private class SchemaProvider<T> : ISchemaProvider
            where T : BasicEntity
        {
            private readonly IDictionary<string, IEnumerable<T>> _values;

            public SchemaProvider(IDictionary<string, IEnumerable<T>> values)
            {
                _values = values;
            }

            public ISchema GetSchema(string schema)
            {
                return new TestSchema<T>(_values[schema]);
            }
        }
    }
}