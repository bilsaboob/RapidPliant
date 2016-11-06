using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Tokens;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Earley
{
    public class TokenViewModel : RapidViewModel
    {
        public IToken Token { get; protected set; }

        public TokenViewModel()
        {
        }

        public string TokenTypeLabel { get { return get(() => TokenTypeLabel); } set { set(() => TokenTypeLabel, value); } }

        public TokenType TokenType { get { return get(() => TokenType); } set { set(() => TokenType, value); } }

        public string Spelling { get { return get(() => Spelling); } set { set(() => Spelling, value); } }

        public int InputPosition { get { return get(() => InputPosition); } set { set(() => InputPosition, value); } }

        public TokenViewModel LoadForToken(IToken token)
        {
            if (token == null)
                return this;

            Token = token;

            TokenType = Token.TokenType;
            Spelling = Token.Value;
            InputPosition = Token.Position;

            TokenTypeLabel = $"{TokenType.Id}";

            return this;
        }
    }
}
