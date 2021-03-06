﻿using System;
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

                        continue;
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
                            /*if(stackEvent.Pushed &&
                                stackEvent.State.IsSimpleValue == true
                                )
                            {

                            }*/


                            if (stackEvent.Pushed /*&&
                                stackEvent.State.IsSimpleValue == false*/)
                            {
                                S4JToken prevVal = valueStack.Peek();
                                S4JToken newToken = new S4JTokenFactory().To_token(stackEvent.State);
                                newToken.Parent = prevVal;
                                // if (!stackEvent.State.IsComment)
                                prevVal.AddChildToToken(newToken);
                                valueStack.Push(newToken);
                            }
                            else
                            {
                                if (stackEvent.Chars != null)
                                {
                                    S4JToken currentVal = valueStack.Peek();
                                    currentVal.AppendCharsToToken(stackEvent.Chars);
                                }
                                /* S4JToken currentVal = valueStack.Peek();
                                 if(!(currentVal is S4JTokenTextValue))
                                 {
                                     S4JTokenTextValue textToken = new S4JTokenTextValue();
                                     textToken.Parent = currentVal;
                                     currentVal.AddChildToToken(textToken);
                                     valueStack.Push(textToken);
                                 }
                                 currentVal = valueStack.Peek();
                                 currentVal.AppendCharsToToken(stackEvent.Chars);*/
                            }

                            /* if (stackEvent.Chars != null)
                             {
                                 S4JToken currentVal = valueStack.Peek();
                                 currentVal.AppendCharsToToken(stackEvent.Chars);
                             }*/

                        }

                        continue;
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
            S4JToken parentToken = stateStack.Peek();
            if (GetStateEnd(code, index, StateBag, parentToken) != null)
            {
                Int32 nextIndex =
                    index +
                    (parentToken.State.FoundGates.First().End == null ||
                     parentToken.State.FoundGates.First().OmitEnd ?
                        0 :
                        (parentToken.State.FoundGates.First().End.Length - 1)) + 1;

                yield return new S4JStateStackEvent()
                {
                    NewIndex = S4JParserHelper.SkipWhiteSpaces(code, nextIndex),
                    State = parentToken.State,
                    Popped = true,
                };

                yield break;
            }

            parentToken = stateStack.Peek();
            S4JState newState = GetStateBegin(code, index, StateBag, parentToken);
            if (newState != null)
            {
                Int32 nextIndex = index + (newState.FoundGates?.FirstOrDefault()?.Start == null ?
                    0 :
                    (newState.FoundGates.First().Start.Length - 1)) + 1;

                yield return new S4JStateStackEvent()
                {
                    NewIndex = newState.IsQuotation ?
                            nextIndex :
                            S4JParserHelper.SkipWhiteSpaces(code, nextIndex),
                    State = newState,
                    Pushed = true
                };
            }
            else
            {
                Int32? newIndex = null;

                // pominiecie białych znaków do nastepnego 'stanu'
                // tylko jesli nie nie jestesmy w cytacie
                if (!parentToken.State.IsQuotation/* &&
                    !prevToken.State.IsComment*/)
                {
                    newIndex = S4JParserHelper.SkipWhiteSpaces(code, index + 1);
                    if (newIndex != null)
                    {
                        S4JState stateBegin = GetStateBegin(code, newIndex.Value, StateBag, parentToken);
                        S4JState stateEnd = GetStateEnd(code, newIndex.Value, StateBag, parentToken);

                        if (stateBegin == null && stateEnd == null)
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

        private static S4JState GetStateBegin(char[] code, Int32 index, S4JStateBag StateBag, S4JToken ParentToken)
        {
            if (ParentToken == null)
                return null;

            Boolean isAllowed = false;
            List<S4JStateGate> matchedGates = new List<S4JStateGate>();

            S4JState foundState = null;

            // pobszukiwanie rozpoczecia stanu
            S4JState[] allowedStates = StateBag.GetAllowedStates(ParentToken?.State);
            for (var i = 0; i < allowedStates.Length; i++)
            {
                var state = allowedStates[i];

                Int32 gateStartCount = 0;
                foreach (S4JStateGate gate in state.Gates)
                {
                    if (S4JParserHelper.Is(code, index, gate.Start))
                    {
                        if (CheckPrevTokens(state, ParentToken))
                        {
                            // jesli znaleziony ciag jest wiekszy od ostatniego
                            if (gateStartCount < gate.Start.Length && matchedGates.Count > 0)
                                matchedGates.Clear();

                            matchedGates.Add(gate.Clone());
                            isAllowed = true;
                            foundState = state;
                            gateStartCount = gate.Start.Length;
                        }
                    }
                }
            }

            if (isAllowed)
            {
                S4JState newState = foundState.Clone();
                newState.FoundGates = matchedGates;
                return newState;
            }

            return null;
        }

        private static bool CheckPrevTokens(S4JState StateToCheck, S4JToken ParentToken)
        {
            if (StateToCheck.RequiredPrevStatesNames == null ||
                StateToCheck.RequiredPrevStatesNames.Count == 0)
                return true;

            foreach (EStateType[] stateTypes in StateToCheck.RequiredPrevStatesNames)
            {
                Boolean result = false;
                S4JToken currentTokenToCheck = ParentToken.GetLastVisibleChild();
                for (var i = stateTypes.Length - 1; i >= 0; i--)
                {
                    EStateType stateType = stateTypes[i];
                    if (currentTokenToCheck == null ||
                        currentTokenToCheck.State.StateType != stateType)
                    {
                        result = false;
                        break;
                    }

                    result = true;
                    ParentToken = ParentToken.PrevToken;
                }

                if (result)
                    return true;
            }

            return false;
        }
    }
}
