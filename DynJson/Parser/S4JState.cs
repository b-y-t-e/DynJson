using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Parser
{
    public class S4JState
    {
        private HashSet<EStateType> allowedStatesNames;
        public ICollection<EStateType> AllowedStateTypes
        {
            get { return allowedStatesNames; }
            set
            {
                allowedStatesNames = value == null ? null : new HashSet<EStateType>(value);
            }
        }

        //public IList<S4JState> AllowedStates { get; set; }

        public List<S4JStateGate> Gates { get; set; }

        public List<S4JStateGate> FoundGates { get; set; }

        public EStateType StateType { get; set; }

        public Int32 Priority { get; set; }

        //////////////////////////////////////////

        public Boolean IsCollection { get; set; }

        public Boolean IsValue { get; set; }

        public Boolean IsSimpleValue { get; set; }

        public Boolean IsFunction { get; set; }

        public Boolean IsQuotation { get; set; }

        public Boolean IsComment { get; set; }

        public Boolean IsDelimiter { get; set; }

        public Boolean IsComa { get; set; }

        public Guid ID { get; set; }

        ////////////////////////////////

        public S4JState()
        {
            ID = Guid.NewGuid();
            AllowedStateTypes = new EStateType[0];
            Gates = new List<S4JStateGate>();
        }

        ////////////////////////////////

        public bool IsAllowed(S4JState State)
        {
            return IsAllowed(State.StateType);
        }

        private bool IsAllowed(EStateType StateType)
        {
            if (allowedStatesNames.Contains(StateType))
                return true;

            if (allowedStatesNames.Contains(EStateType.ANY))
                return true;

            return false;
        }

        public S4JState Clone()
        {
            S4JState item = (S4JState)this.MemberwiseClone();
            item.AllowedStateTypes = this.AllowedStateTypes;
            item.Gates = this.Gates.ToList();
            item.FoundGates = null;
            return item;
        }
    }

    public class S4JStateGate
    {
        public char[] Start { get; set; }

        public char[] End { get; set; }

        public char[] Inner { get; set; }

        public S4JStateGate Clone()
        {
            S4JStateGate item = (S4JStateGate)this.MemberwiseClone();
            return item;
        }
    }

    public class S4JStateStackEvent
    {
        public Int32? NewIndex
        {
            get;
            set;
        }

        public IList<Char> Chars
        {
            get;
            set;
        }

        public S4JState State
        {
            get;
            set;
        }

        public Boolean Pushed
        {
            get;
            set;
        }

        public Boolean Popped
        {
            get;
            set;
        }

        public Boolean AnyChange
        {
            get { return Pushed || Popped; }
        }
    }

    public enum EStateType
    {
        ANY = 0,

        S4J = 1,
        S4J_COMMENT = 2,
        S4J_QUOTATION = 3,
        S4J_ARRAY = 4,

        S4J_TEXT_VALUE = 5,
        S4J_OBJECT_CONTENT = 6,
        S4J_OBJECT = 7,
        S4J_PARAMETERS = 8,
        S4J_VARIABLE = 16,

        FUNCTION = 9,
        FUNCTION_COMMENT = 10,
        FUNCTION_BRACKETS = 11,
        FUNCTION_QUOTATION = 12,

        S4J_VALUE_DELIMITER = 13,
        S4J_COMA = 14,
        S4J_TAG = 15
    }

}


