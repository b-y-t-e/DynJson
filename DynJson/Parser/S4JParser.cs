using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DynJson.Tokens;

namespace DynJson.Parser
{
    public class S4JParser
    {
        public S4JTokenRoot Parse(String Text, S4JStateBag stateBag)
        {
            char[] chars = Text.Trim().ToCharArray();

            S4JTokenStack valueStack = new S4JTokenStack();
            S4JTokenRoot rootVal = new S4JTokenRoot()
            {
                State = stateBag.RootState
            };
            valueStack.Push(rootVal);

            Int32 startIndex = S4JParserHelper.
                SkipWhiteSpaces(chars, 0) ?? Int32.MaxValue;

            for (int i = startIndex; i < chars.Length; i++)
            {
                foreach (S4JStateStackEvent stackEvent in Analyse(chars, i, stateBag, valueStack))
                {
                    if (stackEvent.NewIndex != null)
                        i = stackEvent.NewIndex.Value - 1;

                    // zdjęcie ze stosu
                    if (stackEvent.Popped)
                    {
                        S4JToken currentVal = valueStack.Peek(); // PeekNonValue();
                        if (stackEvent.Chars != null)
                        {
                            currentVal.AppendCharsToToken(stackEvent.Chars);
                        }

                        // zatwierdzenie tokena
                        currentVal = valueStack.Peek();
                        if (currentVal != null)
                        {
                            currentVal.Commit();
                        }

                        //currentVal.OnPop();
                        valueStack.Pop();
                    }

                    else
                    {
                        if (stackEvent.State.StateType == EStateType.S4J_VALUE_DELIMITER)
                        {
                            if (valueStack.Peek() != null)
                            {
                                valueStack.Peek().MarkLastChildAsObjectKey();
                                valueStack.Peek().Commit();
                            }
                        }

                        else if (stackEvent.State.StateType == EStateType.S4J_COMA)
                        {
                            if (valueStack.Peek() != null)
                            {
                                valueStack.Peek().Commit();
                            }
                        }

                        else
                        {

                            if (stackEvent.Pushed &&
                                stackEvent.State.IsSimpleValue == false)
                            {
                                S4JToken prevVal = valueStack.Peek();
                                S4JToken newToken = new S4JTokenFactory().To_token(stackEvent.State);

                                valueStack.Push(newToken);

                                newToken.Parent = prevVal;

                                if (!stackEvent.State.IsComment)
                                    prevVal.AddChildToToken(newToken);
                            }

                            if (stackEvent.Chars != null)
                            {
                                S4JToken currentVal = valueStack.Peek();
                                currentVal.AppendCharsToToken(stackEvent.Chars);
                            }

                        }
                    }
                }
            }

            while (valueStack.Count > 0)
            {
                S4JToken currentVal = valueStack.Peek();
                if (currentVal != null)
                    currentVal.Commit();
                //currentVal.OnPop();
                valueStack.Pop();
            }

            /*if (String.IsNullOrEmpty(rootVal.Name))
            {
                return rootVal.Children.Single() as S4JToken;
            }*/

            return rootVal;
        }

        private static IEnumerable<S4JStateStackEvent> Analyse(char[] code, int index, S4JStateBag StateBag, S4JTokenStack stateStack) // S4JStateStack stateStack)
        {
            // sprawdzamy zakończenie stanu
            S4JToken prevToken = stateStack.Peek();
            if (GetStateEnd(code, index, StateBag, prevToken) != null)
            {
                Int32 nextIndex = index + (prevToken.State.FoundGates.First().End == null ? 0 : (prevToken.State.FoundGates.First().End.Length - 1)) + 1;

                yield return new S4JStateStackEvent()
                {
                    NewIndex = S4JParserHelper.SkipWhiteSpaces(code, nextIndex),
                    State = prevToken.State,
                    Popped = true,
                    // Chars = end
                };

                yield break;
            }

            prevToken = stateStack.Peek();
            S4JState state = GetStateBegin(code, index, StateBag, prevToken);
            if (state != null)
            {
                Int32 nextIndex = index + (state.FoundGates?.FirstOrDefault()?.Start == null ?
                    0 :
                    (state.FoundGates.First().Start.Length - 1)) + 1;

                yield return new S4JStateStackEvent()
                {
                    // NewIndex = null,
                    NewIndex = state.IsQuotation ?
                            nextIndex :
                            // state.IsCollection ?
                            S4JParserHelper.SkipWhiteSpaces(code, nextIndex),
                    //   index + (matchedGate?.Start == null ? 0 : (matchedGate.Start.Count - 1)) + 1,
                    State = state,
                    Pushed = true,
                    // Chars = matchedGate?.Start ?? new[] { code[index] }
                };
            }
            else
            {
                Int32? newIndex = null;

                // pominiecie białych znaków do nastepnego 'stanu'
                // tylko jesli nie nie jestesmy w cytacie
                if (!prevToken.State.IsQuotation &&
                    !prevToken.State.IsComment)
                {
                    newIndex = S4JParserHelper.SkipWhiteSpaces(code, index + 1);
                    if (newIndex != null)
                    {
                        S4JState stateBegin = GetStateBegin(code, newIndex.Value, StateBag, prevToken);
                        S4JState stateEnd = GetStateEnd(code, newIndex.Value, StateBag, prevToken);

                        if (stateBegin != null || stateEnd != null)
                        {
                            newIndex = newIndex;
                        }
                        else
                        {
                            newIndex = null;
                        }
                    }
                    else
                    {
                        newIndex = Int32.MaxValue;
                    }
                }

                yield return new S4JStateStackEvent()
                {
                    NewIndex = newIndex,
                    State = stateStack.Peek()?.State,
                    Chars = new[] { code[index] }
                };
            }
        }

        private static S4JState GetStateEnd(char[] code, Int32 index, S4JStateBag StateBag, S4JToken prevToken)
        {
            if (prevToken == null)
                return null;

            if (prevToken?.State?.FoundGates == null)
                return null;

            foreach (var gate in prevToken.State.FoundGates)
            {
                // TODO GATE
                char[] end = gate.End;
                if (S4JParserHelper.Is(code, index, end))
                {
                    prevToken.State.FoundGates.RemoveAll(g => g != gate);

                    return prevToken?.State;
                }
            }

            return null;
        }

        private static S4JState GetStateBegin(char[] code, Int32 index, S4JStateBag StateBag, S4JToken prevToken)
        {
            if (prevToken == null)
                return null;

            //foreach (S4JState state in StateBag.GetStates(prevToken.State.AllowedStatesNames))
            {
                // sprawdzamy rozpoczecie stanu
                // if (prevToken == null /*||
                //    prevToken.State.IsAllowed(state)*///)
                {
                    Boolean isAllowed = false;
                    List<S4JStateGate> matchedGates = new List<S4JStateGate>();

                    S4JState foundState = null;

                    // pobszukiwanie rozpoczecia stanu
                    var allowedStates = StateBag.GetAllowedStates(prevToken?.State);
                    //foreach (S4JState state in )
                    for (var i = 0; i < allowedStates.Length; i++)
                    {
                        var state = allowedStates[i];

                        Int32 gateStartCount = 0;
                        foreach (S4JStateGate gate in state.Gates)
                        {
                            if (S4JParserHelper.Is(code, index, gate.Start))
                            {
                                // jesli znaleziony ciag jest wiekszy od ostatniego
                                if (gateStartCount < gate.Start.Length && matchedGates.Count > 0)
                                    matchedGates.Clear();
                                matchedGates.Add(gate.Clone());
                                isAllowed = true;
                                foundState = state;
                                gateStartCount = gate.Start.Length;
                                //break;
                            }
                        }

                        //if (isAllowed)
                        //    break;
                    }

                    if (isAllowed)
                    {
                        S4JState newState = foundState.Clone();
                        newState.FoundGates = matchedGates;
                        return newState;
                    }
                }
            }

            /*foreach (S4JState state in StateBag)
            {
                // sprawdzamy rozpoczecie stanu
                if (prevToken == null ||
                    prevToken.State.IsAllowed(state))
                {
                    Boolean isAllowed = false;
                    List<S4JStateGate> matchedGates = new List<S4JStateGate>();

                    // pobszukiwanie rozpoczecia stanu
                    foreach (S4JStateGate gate in state.Gates)
                    {
                        if (S4JParserHelper.Is(code, index, gate.Start))
                        {
                            matchedGates.Add(gate.Clone());
                            isAllowed = true;
                        }
                    }

                    if (isAllowed)
                    {
                        S4JState newState = state.Clone();
                        newState.FoundGates = matchedGates;
                        return newState;
                    }
                }
            }*/

            return null;
        }
    }
}
