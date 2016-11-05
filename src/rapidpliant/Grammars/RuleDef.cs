using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Builders;
using Pliant.Builders.Expressions;
using Pliant.Grammars;

namespace RapidPliant.Grammars
{
    public interface IGrammarDef
    {
        int Id { get; set; }
    }

    public abstract partial class GrammarDef : IGrammarDef
    {
        public GrammarDef()
        {
        }

        public int Id { get; set; }

        public virtual string Name { get; internal protected set; }

        protected internal abstract BaseExpression Expression { get; }
    }

    public abstract partial class GrammarDef
    {
        #region Expression operators
        public static RuleExpression operator +(GrammarDef lhs, GrammarDef rhs)
        {
            return AddWithAnd(lhs.Expression, rhs.Expression);
        }

        public static RuleExpression operator +(string lhs, GrammarDef rhs)
        {
            return AddWithAnd(new StringLiteralLexerRule(lhs), rhs.Expression);
        }

        public static RuleExpression operator +(GrammarDef lhs, string rhs)
        {
            return AddWithAnd(lhs.Expression, new StringLiteralLexerRule(rhs));
        }

        public static RuleExpression operator +(char lhs, GrammarDef rhs)
        {
            return AddWithAnd(new TerminalLexerRule(lhs), rhs.Expression);
        }

        public static RuleExpression operator +(GrammarDef lhs, char rhs)
        {
            return AddWithAnd(lhs.Expression, new TerminalLexerRule(rhs));
        }

        public static RuleExpression operator +(GrammarDef lhs, BaseLexerRule rhs)
        {
            return AddWithAnd(lhs.Expression, rhs);
        }

        public static RuleExpression operator +(BaseLexerRule lhs, GrammarDef rhs)
        {
            return AddWithAnd(lhs, rhs.Expression);
        }

        public static RuleExpression operator +(BaseTerminal lhs, GrammarDef rhs)
        {
            return AddWithAnd(lhs, rhs.Expression);
        }

        public static RuleExpression operator +(GrammarDef lhs, BaseTerminal rhs)
        {
            return AddWithAnd(lhs.Expression, rhs);
        }

        public static RuleExpression operator |(GrammarDef lhs, GrammarDef rhs)
        {
            return AddWithOr(lhs.Expression, rhs.Expression);
        }

        public static RuleExpression operator |(string lhs, GrammarDef rhs)
        {
            return AddWithOr(new StringLiteralLexerRule(lhs), rhs.Expression);
        }

        public static RuleExpression operator |(GrammarDef lhs, string rhs)
        {
            return AddWithOr(lhs.Expression, new StringLiteralLexerRule(rhs));
        }

        public static RuleExpression operator |(char lhs, GrammarDef rhs)
        {
            return AddWithOr(new TerminalLexerRule(lhs), rhs.Expression);
        }

        public static RuleExpression operator |(GrammarDef lhs, char rhs)
        {
            return AddWithOr(lhs.Expression, new TerminalLexerRule(rhs));
        }

        public static RuleExpression operator |(GrammarDef lhs, BaseLexerRule rhs)
        {
            return AddWithOr(lhs.Expression, rhs);
        }

        public static RuleExpression operator |(BaseLexerRule lhs, GrammarDef rhs)
        {
            return AddWithOr(lhs, rhs.Expression);
        }

        public static RuleExpression operator |(BaseTerminal lhs, GrammarDef rhs)
        {
            return AddWithOr(lhs, rhs.Expression);
        }

        public static RuleExpression operator |(GrammarDef lhs, BaseTerminal rhs)
        {
            return AddWithOr(lhs.Expression, rhs);
        }

        private static RuleExpression AddWithAnd(BaseLexerRule lhs, BaseExpression rhs)
        {
            return AddWithAnd(new SymbolExpression(new LexerRuleModel(lhs)), rhs);
        }

        private static RuleExpression AddWithAnd(BaseExpression lhs, BaseLexerRule rhs)
        {
            return AddWithAnd(lhs, new SymbolExpression(new LexerRuleModel(rhs)));
        }

        private static RuleExpression AddWithAnd(BaseTerminal lhs, BaseExpression rhs)
        {
            return AddWithAnd(new SymbolExpression(new LexerRuleModel(new TerminalLexerRule(lhs, lhs.ToString()))), rhs);
        }

        private static RuleExpression AddWithAnd(BaseExpression lhs, BaseTerminal rhs)
        {
            return AddWithAnd(lhs,new SymbolExpression(new LexerRuleModel(new TerminalLexerRule(rhs, rhs.ToString()))));
        }

        private static RuleExpression AddWithAnd(BaseExpression lhs, BaseExpression rhs)
        {
            var expression = lhs as RuleExpression ?? new RuleExpression(lhs);
            expression.Alterations[expression.Alterations.Count - 1].Add(rhs);
            return expression;
        }

        private static RuleExpression AddWithOr(BaseLexerRule lhs, BaseExpression rhs)
        {
            return AddWithOr(new SymbolExpression(new LexerRuleModel(lhs)), rhs);
        }

        private static RuleExpression AddWithOr(BaseExpression lhs, BaseLexerRule rhs)
        {
            return AddWithOr(lhs, new SymbolExpression(new LexerRuleModel(rhs)));
        }

        private static RuleExpression AddWithOr(BaseTerminal lhs, BaseExpression rhs)
        {
            return AddWithOr(new SymbolExpression(new LexerRuleModel(new TerminalLexerRule(lhs, lhs.ToString()))),rhs);
        }

        private static RuleExpression AddWithOr(BaseExpression lhs, BaseTerminal rhs)
        {
            return AddWithOr(lhs, new SymbolExpression(new LexerRuleModel(new TerminalLexerRule(rhs, rhs.ToString()))));
        }

        private static RuleExpression AddWithOr(BaseExpression lhs, BaseExpression rhs)
        {
            var lhsExpression = (lhs as RuleExpression) ?? new RuleExpression(lhs);
            var rhsExpression = (rhs as RuleExpression) ?? new RuleExpression(rhs);
            foreach (var symbolList in rhsExpression.Alterations)
                lhsExpression.Alterations.Add(symbolList);
            return lhsExpression;
        }
        #endregion
    }

    public interface IRuleDef : IGrammarDef
    {
        ProductionExpression ProductionExpression { get; }
    }

    public abstract class RuleDefBase : GrammarDef
    {
    }

    public partial class RuleDef : RuleDefBase, IRuleDef
    {
        public RuleDef()
        {
        }

        public RuleDef(string ruleName)
        {
            ProductionExpression = new RuleDefProductionExpression(this, new RuleDefNonTerminal(this, ruleName));
        }

        public RuleDef(FullyQualifiedName ruleName)
        {
            ProductionExpression = new RuleDefProductionExpression(this, new RuleDefNonTerminal(this, ruleName));
        }
        
        public override string Name
        {
            get
            {
                return base.Name;
            }
            protected internal set
            {
                if (base.Name != value)
                {
                    ProductionExpression = new RuleDefProductionExpression(this, new RuleDefNonTerminal(this, value));
                    base.Name = value;
                }
            }
        }

        ProductionExpression IRuleDef.ProductionExpression { get { return ProductionExpression; } }
        
        private ProductionExpression ProductionExpression { get; set; }

        protected internal override BaseExpression Expression { get { return ProductionExpression; } }

        public RuleExpression Rule { set { ProductionExpression.Rule = value; } }
    }

    public class RuleDefNonTerminal : NonTerminal
    {
        public RuleDefNonTerminal(RuleDef ruleDef, string @namespace, string name) 
            : base(@namespace, name)
        {
            RuleDef = ruleDef;
        }

        public RuleDefNonTerminal(RuleDef ruleDef, string name)
            : base(name)
        {
            RuleDef = ruleDef;
        }

        public RuleDefNonTerminal(RuleDef ruleDef, FullyQualifiedName fullyQualifiedName) 
            : base(fullyQualifiedName)
        {
            RuleDef = ruleDef;
        }

        public RuleDef RuleDef { get; private set; }
    }

    public class RuleDefProductionExpression : ProductionExpression
    {
        public RuleDefProductionExpression(RuleDef ruleDef, INonTerminal leftHandSide)
            : base(leftHandSide)
        {
            RuleDef = ruleDef;
        }

        public RuleDefProductionExpression(RuleDef ruleDef, FullyQualifiedName fullyQualifiedName)
            : base(fullyQualifiedName)
        {
            RuleDef = ruleDef;
        }

        public RuleDef RuleDef { get; private set; }
    }

    public partial class RuleDef
    {
        public static implicit operator RuleDef(string ruleName)
        {
            return new RuleDef(ruleName);
        }

        public static implicit operator RuleDef(FullyQualifiedName fullyQualifiedName)
        {
            return new RuleDef(fullyQualifiedName);
        }

        public static implicit operator ProductionExpression(RuleDef ruleDef)
        {
            return ruleDef.ProductionExpression;
        }
    }

    public interface ILexDef : IGrammarDef
    {
        ILexerRule LexerRule { get; }
        bool Ignore { get; }
    }

    public abstract class LexDefBase : GrammarDef
    {
    }

    public partial class LexDef : LexDefBase, ILexDef
    {
        private BaseLexerRule _lexRule;
        private SymbolExpression _symbolExpression;

        public LexDef()
        {
        }
        
        public bool Ignore { get; set; }
        
        ILexerRule ILexDef.LexerRule { get { return Rule; } }
        
        public BaseLexerRule Rule
        {
            get
            {
                EnsureLexRule();
                return _lexRule;
            }
            set
            {
                _lexRule = value;
            }
        }

        protected SymbolExpression SymbolExpression
        {
            get
            {
                EnsureLexRule();

                if (_symbolExpression == null)
                {
                    _symbolExpression = new SymbolExpression(new LexerDefModel(this));
                }

                return _symbolExpression;
            }
        }

        protected internal override BaseExpression Expression { get { return SymbolExpression; } }

        private void EnsureLexRule()
        {
            if (_lexRule == null)
                throw new Exception($"Lex rule has not been initialized! make sure to set the 'Rule' property of the lex definition '{Name}'!");
        }
    }

    public partial class LexDef : LexDefBase
    {
        public static implicit operator RuleExpression(LexDef lexDef)
        {
            return new RuleExpression(lexDef.SymbolExpression);
        }
    }

    public class LexerDefModel : LexerRuleModel
    {
        public LexerDefModel(LexDef lexDef)
            : base(lexDef.Rule)
        {
            LexDef = lexDef;
        }

        public LexDef LexDef { get; private set; }
    }
}
