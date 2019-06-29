using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Parser
{
    public class S4JState
    {
        public Guid ID
        {
            get;
            set;
        }

        //////////////////////////////////////////

        private HashSet<EStateType> allowedStatesNames;
        public ICollection<EStateType> AllowedStateTypes
        {
            get { return allowedStatesNames; }
            set { allowedStatesNames = value == null ? null : new HashSet<EStateType>(value); }
        }

        //////////////////////////////////////////

        private HashSet<EStateType[]> requiredPrevStatesNames;
        public ICollection<EStateType[]> RequiredPrevStatesNames
        {
            get { return requiredPrevStatesNames; }
            set { requiredPrevStatesNames = value == null ? null : new HashSet<EStateType[]>(value); }
        }

        //////////////////////////////////////////

        public List<S4JStateGate> Gates { get; set; }

        public List<S4JStateGate> FoundGates { get; set; }

        public EStateType StateType { get; set; }

        public Int32 Priority { get; set; }

        //////////////////////////////////////////

        public Boolean IsValue
        {
            get { return StateType == EStateType.S4J_TEXT_VALUE || StateType == EStateType.S4J_OBJECT_CONTENT; }
        }

        public Boolean IsQuotation
        {
            get { return StateType == EStateType.FUNCTION_QUOTATION || StateType == EStateType.S4J_QUOTATION; }
        }

        public Boolean IsComment
        {
            get { return StateType == EStateType.FUNCTION_COMMENT || StateType == EStateType.S4J_COMMENT; }
        }

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
            item.Gates = this.Gates?.ToList();
            item.FoundGates = this.FoundGates?.ToList();
            return item;
        }
    }

    public class S4JStateGate
    {
        public char[] Start { get; set; }

        public char[] End { get; set; }

        public char[] Inner { get; set; }

        public Boolean OmitEnd { get; set; }

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

        S4J_ROOT = 1,
        S4J_ROOTOBJECT = 2,

        S4J_COMMENT = 102,
        S4J_QUOTATION = 103,
        S4J_ARRAY = 104,

        S4J_TEXT_VALUE = 105,
        S4J_OBJECT_CONTENT = 106,
        S4J_OBJECT = 107,
        S4J_PARAMETERS = 108,

        FUNCTION = 109,
        FUNCTION_COMMENT = 110,
        FUNCTION_BRACKETS = 111,
        FUNCTION_QUOTATION = 112,

        S4J_VALUE_DELIMITER = 113,
        S4J_COMA = 114,
        S4J_TAG = 115
    }

}


