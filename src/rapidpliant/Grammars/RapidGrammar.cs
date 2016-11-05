using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Pliant.Builders;
using Pliant.Builders.Expressions;
using Pliant.Grammars;

namespace RapidPliant.Grammars
{
    public interface IRapidGrammar : IGrammar
    {
    }

    public abstract class RapidGrammar : IRapidGrammar
    {
        private List<GrammarDefEntry> _allDefs;
        private List<GrammarDefEntry> _memberDefs;

        private List<GrammarDefEntry> _lexDefs;
        private List<GrammarDefEntry> _ruleDefs;

        private List<GrammarDefEntry> _startRuleDefs;
        private List<GrammarDefMember> _grammarDefMembers;

        private bool _hasBuilt;

        protected int NextDefinitionId { get; set; }

        public RapidGrammar()
        {
            _allDefs = new List<GrammarDefEntry>();
            _memberDefs = new List<GrammarDefEntry>();
            _ruleDefs = new List<GrammarDefEntry>();
            _lexDefs = new List<GrammarDefEntry>();
            _startRuleDefs = new List<GrammarDefEntry>();

            NextDefinitionId = 1;

            GrammarModel = new GrammarModel();
        }

        protected bool IsBuilt { get { return _hasBuilt; } }
        protected GrammarModel GrammarModel { get; private set; }

        protected List<GrammarDefMember> GrammarDefMembers
        {
            get
            {
                if (_grammarDefMembers == null)
                {
                    _grammarDefMembers = FindGrammarDefMembers();
                }
                return _grammarDefMembers;
            }
        }

        protected abstract void Define();

        protected virtual void OnBuilt()
        {
        }

        private void Build()
        {
            InitializeGrammarDefMembers();

            //Build the grammar!
            Define();

            CollectGrammarDefs();

            EnsureDefNames();

            AssignIds();

            OnBuilt();

            BuildGrammarModel();

            BuildInnerGrammar();

            _hasBuilt = true;
        }

        #region InitializeGrammarDefMembers
        private void InitializeGrammarDefMembers()
        {
            var grammarDefMembers = GrammarDefMembers;

            foreach (var lexDefMember in grammarDefMembers.Where(m => m.IsLexDef))
            {
                var grammarDef = InitializeGrammarDef(lexDefMember);
                AddMemberGrammarDef(lexDefMember, grammarDef);
            }

            foreach (var ruleDefMember in grammarDefMembers.Where(m => m.IsRuleDef))
            {
                var grammarDef = InitializeGrammarDef(ruleDefMember);
                AddMemberGrammarDef(ruleDefMember, grammarDef);
            }

            foreach (var memberDef in _memberDefs)
            {
                AddDefinition(memberDef);
            }
        }

        private void AddMemberGrammarDef(GrammarDefMember defMember, GrammarDef grammarDef)
        {
            if (_memberDefs.Exists(e => e.Member == defMember))
                return;

            _memberDefs.Add(new GrammarDefEntry(defMember, grammarDef));
        }

        private GrammarDef InitializeGrammarDef(GrammarDefMember defMember)
        {
            var grammarDef = EnsureDefinition(defMember);
            EnsureDefinitionName(grammarDef, defMember.MemberName);
            return grammarDef;
        }

        private GrammarDef EnsureDefinition(GrammarDefMember defMember)
        {
            var memberGrammarDef = defMember.GetDef();
            var grammarDef = GetOrCreateGrammarDef(defMember);
            if (grammarDef != memberGrammarDef)
            {
                defMember.SetDef(grammarDef);
            }
            return grammarDef;
        }

        private GrammarDef GetOrCreateGrammarDef(GrammarDefMember defMember)
        {
            var memberGrammarDef = defMember.GetDef();

            if (memberGrammarDef == null)
            {
                var defType = defMember.MemberType;

                if (typeof(LexDef).IsAssignableFrom(defType))
                {
                    memberGrammarDef = new LexDef();
                }
                else if (typeof(RuleDef).IsAssignableFrom(defType))
                {
                    memberGrammarDef = new RuleDef();
                }
            }

            if (memberGrammarDef == null)
            {
                throw new Exception($"Unhandled grammar def member '{defMember.MemberName}' of type '{defMember.MemberType.Name}'!");
            }

            return (GrammarDef)memberGrammarDef;
        }

        private void EnsureDefinitionName(GrammarDef grammarDef, string name)
        {
            if (string.IsNullOrEmpty(grammarDef.Name))
            {
                if (name.Length > 1 && name[0] == 'm' && char.IsUpper(name[1]))
                {
                    name = name.Substring(1);
                }

                //Set the expression name to the name of the field
                grammarDef.Name = name.Trim(' ', '_');
            }
        }
        #endregion

        #region CollectGrammarDefs

        private void CollectGrammarDefs()
        {
            //Collect all grammar defs, member definitions as well as "in place" definitions
            var memberRuleDefEntries = _memberDefs.Where(d => d.Member.IsRuleDef).ToList();
            foreach (var memberRuleDefEntry in memberRuleDefEntries)
            {
                //Add the definition to the "all defs"
                AddDefinition(memberRuleDefEntry);

                //Collect grammar defs for the referenced defs in the expression
                var ruleDef = (IRuleDef)memberRuleDefEntry.GrammarDef;
                CollectGrammarDefsForExpression(ruleDef.ProductionExpression);
            }
        }
        
        private void CollectGrammarDefsForExpression(BaseExpression expr)
        {
            if(expr == null)
                return;

            var ruleDefExpr = expr as RuleDefProductionExpression;
            if (ruleDefExpr != null)
            {
                //Add the rule definition itself, but continue
                AddDefinition(ruleDefExpr.RuleDef);
                CollectGrammarDefsForProductionExpression(ruleDefExpr);
                return;
            }

            //Check for "in place produciton expression" - this has not explicitly been defined by the grammar
            var productionExpr = expr as ProductionExpression;
            if (productionExpr != null)
            {
                CollectGrammarDefsForProductionExpression(productionExpr);
                return;
            }

            //Handle rule expression, collect from alterations
            var ruleExpr = expr as RuleExpression;
            if (ruleExpr != null)
            {
                CollectGrammarDefsForRuleExpression(ruleExpr);
                return;
            }

            //Handle symbol expression, collect def from the symbol
            var symbolRefExpr = expr as SymbolExpression;
            if (symbolRefExpr != null)
            {
                CollectGrammarDefsForSymbol(symbolRefExpr.SymbolModel);
                return;
            }
        }

        private void CollectGrammarDefsForRuleExpression(RuleExpression ruleExpr)
        {
            if(ruleExpr == null)
                return;

            var alts = ruleExpr.Alterations;
            if(alts == null)
                return;

            foreach (var alt in alts)
            {
                if(alt == null)
                    continue;

                foreach (var altExpr in alt)
                {
                    CollectGrammarDefsForExpression(altExpr);
                }
            }
        }

        private void CollectGrammarDefsForProductionExpression(ProductionExpression prodExpr)
        {
            if(prodExpr == null)
                return;

            var alts = prodExpr.ProductionModel.Alterations;
            if (alts == null)
                return;

            foreach (var alt in alts)
            {
                var symbols = alt.Symbols;
                if (symbols == null)
                    continue;

                foreach (var symbol in symbols)
                {
                    CollectGrammarDefsForSymbol(symbol);
                }
            }
        }

        private void CollectGrammarDefsForSymbol(SymbolModel symbol)
        {
            if (symbol == null)
                return;

            var lexDefSymbol = symbol as LexerDefModel;
            if (lexDefSymbol != null)
            {
                AddDefinition(lexDefSymbol.LexDef);
                return;
            }

            var productionModel = symbol as ProductionModel;
            if (productionModel != null)
            {
                var lhsSymbol = productionModel.LeftHandSide.Symbol;
                var lhsRuleDef = lhsSymbol as RuleDefNonTerminal;
                if (lhsRuleDef != null)
                {
                    if (AddDefinition(lhsRuleDef.RuleDef))
                    {
                        //Collect for the rule def too
                        CollectGrammarDefsForExpression(((IRuleDef)lhsRuleDef.RuleDef).ProductionExpression);
                    }
                }
            }
        }

        #endregion

        #region EnsureDefNames
        private void EnsureDefNames()
        {
            foreach (var defEntry in _allDefs)
            {
                var grammarDef = defEntry.GrammarDef;
                if (!string.IsNullOrEmpty(grammarDef.Name))
                    continue;

                if (!defEntry.IsDefinedByMember)
                    throw new Exception($"Definition must have explicit name if no backing member is defined!");

                var initiliaDefinedName = defEntry.MemberGrammarDefName;
                EnsureDefinitionName(grammarDef, initiliaDefinedName);
            }
        }

        #endregion

        #region AssignIds
        private void AssignIds()
        {
            foreach (var defEntry in _allDefs)
            {
                var def = defEntry.GrammarDef;
                if (def.Id != 0)
                    continue;

                def.Id = GenerateNextDefinitionId();
            }
        }

        protected virtual int GenerateNextDefinitionId()
        {
            return NextDefinitionId++;
        }
        #endregion

        #region Build grammar model / inner grammar
        private void BuildGrammarModel()
        {
            //Add each of the rule defs production models to the grammar model
            foreach (var ruleDefEntry in _ruleDefs)
            {
                var ruleDef = (IRuleDef)ruleDefEntry.GrammarDef;
                var production = ruleDef.ProductionExpression.ProductionModel;
                GrammarModel.Productions.Add(production);
            }

            var startDefEntry = _startRuleDefs.LastOrDefault();
            if (startDefEntry != null)
            {
                GrammarModel.Start = ((IRuleDef)startDefEntry.GrammarDef).ProductionExpression.ProductionModel;
            }

            foreach (var lexDefEntry in _lexDefs)
            {
                var lexDef = (ILexDef)lexDefEntry.GrammarDef;
                if (lexDef.Ignore)
                {
                    GrammarModel.IgnoreRules.Add(new LexerRuleModel(lexDef.LexerRule));
                }
            }
        }

        private void BuildInnerGrammar()
        {
            //Create the grammar
            _innerGrammar = GrammarModel.ToGrammar();
        }
        #endregion

        #region Definition helpers
        protected void Start(RuleDef startRuleDef)
        {
            var defEntry = FindDefEntryByDef(startRuleDef);
            if (defEntry == null)
                throw new Exception($"No entry found for the specified start rule definition '{startRuleDef.Name}'!");

            if(!_startRuleDefs.Contains(defEntry))
                _startRuleDefs.Add(defEntry);
        }

        protected void Ignore(LexDef ignoreLexDef)
        {
            var defEntry = FindDefEntryByDef(ignoreLexDef);
            if (defEntry == null)
                throw new Exception($"No entry found for the specified ignore lex definition '{ignoreLexDef.Name}'!");

            //Mark as ignored
            ignoreLexDef.Ignore = true;
        }
        #endregion

        #region Wrapped inner grammar for IGrammar
        private IGrammar _innerGrammar;
        private IGrammar InnerGrammar
        {
            get
            {
                if (_innerGrammar == null)
                {
                    _innerGrammar = EnsureInnerGrammar();
                }

                return _innerGrammar;
            }
        }
        private IGrammar EnsureInnerGrammar()
        {
            if (!IsBuilt)
            {
                Build();
            }
            
            if(_innerGrammar == null)
                throw new Exception("Inner grammar has not been built!");

            return _innerGrammar;
        }
        #endregion

        #region IGrammar
        IReadOnlyList<IProduction> IGrammar.Productions
        {
            get { return InnerGrammar.Productions; }
        }

        INonTerminal IGrammar.Start
        {
            get { return InnerGrammar.Start; }
        }

        IReadOnlyList<ILexerRule> IGrammar.Ignores
        {
            get { return InnerGrammar.Ignores; }
        }

        IReadOnlyList<IProduction> IGrammar.RulesFor(INonTerminal nonTerminal)
        {
            return InnerGrammar.RulesFor(nonTerminal);
        }

        IReadOnlyList<IProduction> IGrammar.StartProductions()
        {
            return InnerGrammar.StartProductions();
        }

        bool IGrammar.IsNullable(INonTerminal nonTerminal)
        {
            return InnerGrammar.IsNullable(nonTerminal);
        }

        IReadOnlyList<IProduction> IGrammar.RulesContainingSymbol(INonTerminal nonTerminal)
        {
            return InnerGrammar.RulesContainingSymbol(nonTerminal);
        }
        #endregion

        #region Internal helpers

        private bool AddDefinition(GrammarDefEntry defEntry)
        {
            if (_allDefs.Contains(defEntry))
                return false;

            _allDefs.Add(defEntry);

            var grammarDef = defEntry.GrammarDef;

            var ruleDef = grammarDef as IRuleDef;
            if (ruleDef != null)
            {
                _ruleDefs.Add(defEntry);
            }

            var lexDef = grammarDef as ILexDef;
            if (lexDef != null)
            {
                _lexDefs.Add(defEntry);
            }

            return true;
        }

        private GrammarDefEntry FindDefEntryByDef(GrammarDef grammarDef)
        {
            return _allDefs.Find(e => e.GrammarDef == grammarDef);
        }

        private GrammarDefEntry FindDefEntryById(int id)
        {
            return _allDefs.Find(e => e.GrammarDef.Id == id);
        }

        private GrammarDefEntry FindDefEntryByName(string name)
        {
            return _allDefs.Find(e => e.GrammarDef.Name == name);
        }

        private bool AddDefinition(GrammarDef grammarDef)
        {
            var defEntry = FindDefEntryByDef(grammarDef);
            if (defEntry == null)
            {
                defEntry = new GrammarDefEntry(grammarDef);
                return AddDefinition(defEntry);
            }
            return false;
        }

        private List<GrammarDefMember> FindGrammarDefMembers()
        {
            var grammarDefMembers = new List<GrammarDefMember>();

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var grammarDefFields = fields.Where(f => typeof(IGrammarDef).IsAssignableFrom(f.FieldType)).ToList();
            grammarDefMembers.AddRange(grammarDefFields.Select(f => new GrammarDefFieldMember(this, f)));

            var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var grammarDefProperties = props.Where(f => typeof(IGrammarDef).IsAssignableFrom(f.PropertyType)).ToList();
            grammarDefMembers.AddRange(grammarDefProperties.Select(p => new GrammarDefPropertyMember(this, p)));

            return grammarDefMembers;
        }

        private class GrammarDefEntry
        {
            public GrammarDefEntry(GrammarDef grammarDef)
                : this(null, grammarDef)
            {
            }

            public GrammarDefEntry(GrammarDefMember member, GrammarDef grammarDef)
            {
                Member = member;
                IsDefinedByMember = Member != null;

                if (IsDefinedByMember)
                {
                    MemberGrammarDefName = grammarDef.Name;
                }

                GrammarDef = grammarDef;
            }

            public GrammarDef GrammarDef { get; set; }

            public bool IsDefinedByMember { get; private set; }
            public GrammarDefMember Member { get; private set; }

            public string MemberGrammarDefName { get; private set; }
        }

        protected abstract class GrammarDefMember
        {
            public GrammarDefMember(IRapidGrammar grammarModel, Type memberType)
            {
                GrammarModel = grammarModel;
                MemberType = memberType;

                if (typeof(ILexDef).IsAssignableFrom(MemberType))
                {
                    IsLexDef = true;
                }
                else if (typeof(IRuleDef).IsAssignableFrom(MemberType))
                {
                    IsRuleDef = true;
                }
            }

            public IRapidGrammar GrammarModel { get; set; }

            public string MemberName { get; protected set; }

            public Type MemberType { get; protected set; }

            public bool IsLexDef { get; protected set; }

            public bool IsRuleDef { get; protected set; }

            public IGrammarDef GetDef(IRapidGrammar grammarModel = null)
            {
                if (grammarModel == null)
                    grammarModel = GrammarModel;

                return GetGrammarDef(grammarModel);
            }

            public void SetDef(IGrammarDef def, IRapidGrammar grammarModel = null)
            {
                if (grammarModel == null)
                    grammarModel = GrammarModel;

                SetGrammarDef(grammarModel, def);
            }

            protected abstract IGrammarDef GetGrammarDef(IRapidGrammar grammarModel);
            protected abstract void SetGrammarDef(IRapidGrammar grammarModel, IGrammarDef def);
        }

        protected class GrammarDefFieldMember : GrammarDefMember
        {
            public GrammarDefFieldMember(IRapidGrammar grammarModel, FieldInfo fieldInfo)
                : base(grammarModel, fieldInfo.FieldType)
            {
                FieldInfo = fieldInfo;
                MemberName = fieldInfo.Name;
            }

            public FieldInfo FieldInfo { get; private set; }

            protected override IGrammarDef GetGrammarDef(IRapidGrammar grammarModel)
            {
                return (IGrammarDef)FieldInfo.GetValue(grammarModel);
            }

            protected override void SetGrammarDef(IRapidGrammar grammarModel, IGrammarDef def)
            {
                FieldInfo.SetValue(grammarModel, def);
            }
        }

        protected class GrammarDefPropertyMember : GrammarDefMember
        {
            public GrammarDefPropertyMember(IRapidGrammar grammarModel, PropertyInfo propInfo)
                : base(grammarModel, propInfo.PropertyType)
            {
                PropertyInfo = propInfo;
                MemberName = propInfo.Name;
            }

            public PropertyInfo PropertyInfo { get; private set; }

            protected override IGrammarDef GetGrammarDef(IRapidGrammar grammarModel)
            {
                return (IGrammarDef)PropertyInfo.GetValue(grammarModel);
            }

            protected override void SetGrammarDef(IRapidGrammar grammarModel, IGrammarDef def)
            {
                PropertyInfo.SetValue(grammarModel, def);
            }
        }
        #endregion
    }
}
