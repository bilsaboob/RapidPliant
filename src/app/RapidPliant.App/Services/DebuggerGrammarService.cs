﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Grammars;
using Pliant.Json;
using Pliant.RegularExpressions;

namespace RapidPliant.App.Services
{
    public interface IDebuggerGrammarService
    {
        IGrammar GetGrammarByType(Type grammarType);
        IGrammar GetGrammarByName(string grammarName);

        IEnumerable<GrammarInfo> GetAvailableGrammars();
    }

    public class GrammarInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Type GrammarType { get; set; }
    }

    public class DebuggerGrammarService : IDebuggerGrammarService
    {
        public DebuggerGrammarService()
        {
        }

        public IGrammar GetGrammarByType(Type grammarType)
        {
            return Activator.CreateInstance(grammarType) as IGrammar;
        }

        public IGrammar GetGrammarByName(string grammarName)
        {
            return new JsonGrammar();
        }

        public IEnumerable<GrammarInfo> GetAvailableGrammars()
        {
            yield return new GrammarInfo() {
                Name = "Json grammar",
                Description = "Grammar for parsing json",
                GrammarType = typeof(JsonGrammar)
            };
        }
    }
}