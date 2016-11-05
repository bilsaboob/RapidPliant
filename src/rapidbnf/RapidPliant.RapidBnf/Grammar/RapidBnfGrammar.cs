using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Automata;
using Pliant.Builders.Expressions;
using Pliant.Grammars;
using Pliant.LexerRules;
using Pliant.RegularExpressions;
using RapidPliant.Grammars;

namespace RapidPliant.RapidBnf.Grammar
{
    public class RapidBnfGrammar : RapidGrammar
    {
        RuleDef
            //Rule
            RuleDecl,
            RuleDeclArgs,
            RuleDeclArg,
            RuleDef,
            RuleExpr,
            SpellingExpr,
            ReferenceExpr
            ;

        LexDef
            ruleNameIdent,
            ruleDeclArgIdent,
            ruleRefIdent,
            spelling
            ;

        LexDef
            whitespace
            ;

        protected override void Define()
        {
            #region Lexing

            ruleNameIdent.Rule = 
                Pattern("([a-zA-Z0-9_])+");

            ruleDeclArgIdent.Rule = 
                Pattern("([a-zA-Z0-9_])+");

            spelling.Rule =
                Pattern("([a-zA-Z0-9_])+");

            ruleRefIdent.Rule =
                Pattern("([a-zA-Z0-9_])+");

            whitespace.Rule = Whitespace();

            //Mark the whitespace as an ignore
            Ignore(whitespace);

            #endregion

            #region Productions

            RuleDecl.Rule =
                    ruleNameIdent + ":" + RuleDef
                |   ruleNameIdent + RuleDeclArgs + ":"
                ;

            RuleDeclArgs.Rule =
                    "(" + RuleDeclArg + ")"
                ;

            RuleDeclArg.Rule =
                    ruleDeclArgIdent
                ;

            RuleDef.Rule =
                    RuleExpr
                |   RuleExpr + RuleDef
                ;

            RuleExpr.Rule =
                    SpellingExpr
                |   ReferenceExpr
                ;

            ReferenceExpr.Rule =
                    ruleRefIdent
                ;

            SpellingExpr.Rule =
                    "\"" + spelling + "\""
                ;
            
            #endregion

            Start(RuleDecl);
        }
        
        private BaseLexerRule Whitespace()
        {
            return new WhitespaceLexerRule();
        }

        private BaseLexerRule Pattern(string pattern)
        {
            var regexParser = new RegexParser();
            var regex = regexParser.Parse(pattern);
            var regexCompiler = new RegexCompiler();
            var dfa = regexCompiler.Compile(regex);
            return new DfaLexerRule(dfa, pattern);
        }

        private ProductionExpression list(ProductionExpression repeatProduction, string separatorSpelling)
        {
            return repeatProduction;
        }
    }
}
