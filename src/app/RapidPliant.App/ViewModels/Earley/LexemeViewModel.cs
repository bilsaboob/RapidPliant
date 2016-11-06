using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Grammars;
using Pliant.Tokens;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Earley
{
    public class LexemeViewModel : RapidViewModel
    {
        public ILexeme Lexeme { get; protected set; }

        public LexemeViewModel()
        {
        }

        public string DisplayLabel { get { return get(() => DisplayLabel); } set { set(() => DisplayLabel, value); } }

        public LexerRuleType LexerRuleType { get { return get(() => LexerRuleType); } set { set(() => LexerRuleType, value); } }

        public TokenType TokenType { get { return get(() => TokenType); } set { set(() => TokenType, value); } }

        public bool IsNew { get { return get(() => IsNew); } set { set(() => IsNew, value); } }

        public bool IsAccepted { get { return get(() => IsAccepted); } set { set(() => IsAccepted, value); } }

        public string Spelling { get { return get(() => Spelling); } set { set(() => Spelling, value); } }
        
        public LexemeViewModel LoadForLexeme(ILexeme lexeme)
        {
            if (lexeme == null)
                return this;

            Lexeme = lexeme;

            LexerRuleType = Lexeme.LexerRule.LexerRuleType;

            TokenType = Lexeme.TokenType;

            Spelling = Lexeme.Value;

            DisplayLabel = ToLexemeDisplayString();

            return this;
        }

        public string ToLexemeDisplayString()
        {
            return $"'{Lexeme.Value}' => {TokenType.Id} : {LexerRuleType.Id}";
        }
    }
}
