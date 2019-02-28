using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DynJson.Parser
{
    public class S4JStateBag : IEnumerable<S4JState> //: List<S4JState>
    {
        public S4JState RootState { get; private set; }

        public S4JState ValueState { get; private set; }

        ////////////////////////////////////////

        private List<S4JState> all_states;

        private Dictionary<Guid, S4JState> all_states_dict;

        private Dictionary<EStateType, List<S4JState>> stateType_states;

        private Dictionary<Guid, S4JState[]> stateID_allowed_states;

        ////////////////////////////////////////

        public S4JStateBag()
        {
            all_states = new List<S4JState>();
            all_states_dict = new Dictionary<Guid, S4JState>();
            stateType_states = new Dictionary<EStateType, List<S4JState>>();
            stateID_allowed_states = new Dictionary<Guid, S4JState[]>();

            RootState = AddState(new S4JState()
            {
                Priority = -1000,
                StateType = EStateType.S4J,
                AllowedStateTypes = new[]
                {
                    EStateType.S4J_COMMENT,
                    EStateType.S4J_QUOTATION,
                    EStateType.S4J_OBJECT,
                    EStateType.S4J_ARRAY,
                    EStateType.S4J_VALUE_DELIMITER,
                    EStateType.S4J_COMA,
                    EStateType.S4J_TEXT_VALUE,
                    EStateType.FUNCTION,
                    EStateType.S4J_PARAMETERS,
                    EStateType.S4J_TAG
                },
                Gates = new List<S4JStateGate>()
                {

                }
            });

            ////////////////////////////////

            AddState(new S4JState()
            {
                Priority = -999,
                StateType = EStateType.S4J_COMMENT,
                AllowedStateTypes = new[]
                {
                    EStateType.S4J_COMMENT
                },
                IsComment = true,
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "/*".ToCharArray(),
                        End = "*/".ToCharArray()
                    },
                    new S4JStateGate()
                    {
                        Start = "//".ToCharArray(),
                        End = "\n".ToCharArray()
                    }
                }
            });

            ////////////////////////////////

            AddState(new S4JState()
            {
                Priority = -998,
                StateType = EStateType.S4J_QUOTATION,
                IsValue = true,
                IsQuotation = true,
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "'".ToCharArray(),
                        End = "'".ToCharArray(),
                        Inner = "\\".ToCharArray()
                    },
                    new S4JStateGate()
                    {
                        Start = "\"".ToCharArray(),
                        End = "\"".ToCharArray(),
                        Inner = "\\".ToCharArray()
                    }
                }
            });

            ////////////////////////////////

            AddState(new S4JState()
            {
                Priority = 1000,
                StateType = EStateType.S4J_ARRAY,
                IsCollection = true,
                AllowedStateTypes = new[]
                {
                    EStateType.S4J_COMMENT,
                    EStateType.S4J_QUOTATION,
                    EStateType.S4J_COMA,
                    EStateType.S4J_OBJECT,
                    EStateType.S4J_ARRAY,
                    EStateType.S4J_TEXT_VALUE,
                    EStateType.FUNCTION,
                    EStateType.S4J_TAG
                },
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "[".ToCharArray(),
                        End = "]".ToCharArray()
                    }
                }
            });

            ////////////////////////////////

            AddState(new S4JState()
            {
                Priority = 2000,
                StateType = EStateType.S4J_OBJECT,
                IsCollection = true,
                AllowedStateTypes = new[]
                {
                    EStateType.S4J_COMMENT,
                    EStateType.S4J_QUOTATION,
                    EStateType.S4J_OBJECT,
                    EStateType.S4J_ARRAY,
                    EStateType.S4J_VALUE_DELIMITER,
                    EStateType.S4J_COMA,
                    EStateType.S4J_TEXT_VALUE,
                    EStateType.FUNCTION,
                    EStateType.S4J_TAG
                },
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "{".ToCharArray(),
                        End = "}".ToCharArray()
                    }
                }
            });

            ////////////////////////////////

            AddState(new S4JState()
            {
                Priority = 2500,
                StateType = EStateType.S4J_PARAMETERS,
                IsCollection = true,
                AllowedStateTypes = new[]
                {
                    EStateType.S4J_COMMENT,
                    EStateType.S4J_QUOTATION,
                    EStateType.S4J_OBJECT,
                    EStateType.S4J_ARRAY,
                    EStateType.S4J_VALUE_DELIMITER,
                    EStateType.S4J_COMA,
                    EStateType.S4J_TEXT_VALUE,
                    EStateType.S4J_TAG
                },
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "(".ToCharArray(),
                        End = ")".ToCharArray()
                    }
                }
            });

            ////////////////////////////////

            AddState(new S4JState()
            {
                Priority = 2600,
                StateType = EStateType.S4J_TAG,
                IsCollection = true,
                AllowedStateTypes = new[]
                {
                    //EStateType.S4J_COMMENT,
                    //EStateType.S4J_QUOTATION,
                    //EStateType.S4J_OBJECT,
                    //EStateType.S4J_ARRAY,
                    EStateType.S4J_VALUE_DELIMITER,
                    //EStateType.S4J_COMA,
                    EStateType.S4J_TEXT_VALUE
                },
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "#".ToCharArray(),
                        End = " ".ToCharArray()
                    },
                    new S4JStateGate()
                    {
                        Start = "#".ToCharArray(),
                        End = "\n".ToCharArray()
                    },
                    new S4JStateGate()
                    {
                        Start = "#".ToCharArray(),
                        End = "\r".ToCharArray()
                    },
                    new S4JStateGate()
                    {
                        Start = "(#".ToCharArray(),
                        End = ")".ToCharArray()
                    }
                }
            });

            ////////////////////////////////

            AddState(new S4JState()
            {
                Priority = 3000,
                StateType = EStateType.S4J_VALUE_DELIMITER,
                AllowedStateTypes = new[]
                {
                    EStateType.ANY
                },
                IsDelimiter = true,
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = ":".ToCharArray()
                    }
                }
            });

            ////////////////////////////////

            AddState(new S4JState()
            {
                Priority = 4000,
                StateType = EStateType.S4J_COMA,
                AllowedStateTypes = new[]
                {
                    EStateType.ANY
                },
                IsComa = true,
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = ",".ToCharArray()
                    }
                }
            });

            ////////////////////////////////

            this.ValueState = AddState(new S4JState()
            {
                Priority = 5000,
                StateType = EStateType.S4J_TEXT_VALUE,
                AllowedStateTypes = new[]
                {
                    EStateType.ANY
                },
                IsValue = true,
                IsSimpleValue = true,
                Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        End = ",".ToCharArray(),
                        OmitEnd = true
                    },
                    new S4JStateGate()
                    {
                        End = ":".ToCharArray(),
                        OmitEnd = true
                    },
                    new S4JStateGate()
                    {
                        End = "}".ToCharArray(),
                        OmitEnd = true
                    },
                    new S4JStateGate()
                    {
                        End = "]".ToCharArray(),
                        OmitEnd = true
                    }
                }
            });

            CorrectItems();
            CorrectOrderOfItems();
        }

        ////////////////////////////////////

        public S4JStateBag Clone()
        {
            S4JStateBag item = new S4JStateBag();
            item.all_states = this.all_states.Select(i => i.Clone()).ToList();
            item.all_states_dict = item.all_states.ToDictionary(i => i.ID, i => i);
            item.RootState = item.all_states_dict[this.RootState.ID];
            item.ValueState = item.all_states_dict[this.ValueState.ID];
            item.stateType_states = this.stateType_states.ToDictionary(i => i.Key, i => i.Value.Select(v => item.all_states_dict[v.ID]).ToList());
            item.stateID_allowed_states = this.stateID_allowed_states.ToDictionary(i => i.Key, i => i.Value.Select(v => item.all_states_dict[v.ID]).ToArray());
            return item;
        }

        public S4JState[] GetAllowedStates(S4JState State)
        {
            if (State == null)
                return new S4JState[0];

            if (stateID_allowed_states.ContainsKey(State.ID))
                return this.stateID_allowed_states[State.ID];

            return new S4JState[0];

        }

        public void AddStatesToBag(params S4JState[] States)
        {
            foreach (S4JState state in States)
            {
                AddState(state);
                Correct(state);
            }
            CorrectDependent(States);
            CorrectOrderOfItems();
        }

        public void AddStatesToBag(params S4JStateFunction[] States)
        {
            foreach (S4JStateFunction state in States)
            {
                AddState(state);
                AddState(state.BracketsDefinition);
                AddState(state.CommentDefinition);
                AddState(state.QuotationDefinition);

                Correct(state);
                Correct(state.CommentDefinition);
                Correct(state.BracketsDefinition);
                Correct(state.QuotationDefinition);
            }
            CorrectDependent(States);
            CorrectOrderOfItems();
        }

        ////////////////////////////////////

        S4JState AddState(S4JState state)
        {
            this.all_states.Add(state);
            this.all_states_dict[state.ID] = state;

            if (!this.stateType_states.ContainsKey(state.StateType))
                this.stateType_states[state.StateType] = new List<S4JState>();

            this.stateType_states[state.StateType].Add(state);
            return state;
        }

        void CorrectItems()
        {
            foreach (S4JState state in all_states)
                Correct(state);
        }

        void CorrectDependent(IEnumerable<S4JState> States)
        {
            foreach (EStateType stateType in States.Select(s => s.StateType))
                foreach (S4JState state in all_states)
                    if (state.AllowedStateTypes.Contains(stateType))
                        Correct(state);
        }

        void Correct(S4JState State)
        {
            if (State == null)
                return;

            this.stateID_allowed_states[State.ID] = State.AllowedStateTypes == null ? null :
                (State.AllowedStateTypes.
                    Where(i => stateType_states.ContainsKey(i)).
                    SelectMany(i => stateType_states[i]).
                    OrderBy(i => i.Priority).
                    ToArray());
        }

        void CorrectOrderOfItems()
        {
            this.all_states = this.all_states.
                OrderBy(i => i.Priority).
                ToList();
        }

        public IEnumerator<S4JState> GetEnumerator()
        {
            return (IEnumerator<S4JState>)all_states.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)all_states.GetEnumerator();
        }
    }
}
