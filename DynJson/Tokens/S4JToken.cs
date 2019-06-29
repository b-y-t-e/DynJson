using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DynJson.Helpers;
using System.Collections;
using DynJson.Helpers.CoreHelpers;
using DynJson.Parser;

namespace DynJson.Tokens
{
    public abstract class S4JToken
    {
        public S4JToken Parent { get; set; }

        public S4JToken PrevToken { get; set; }

        public S4JToken NextToken { get; set; }

        public List<S4JToken> Children { get; set; }

        public Dictionary<String, Object> Tags { get; set; }

        public S4JState State { get; set; }

        public Boolean IsVisible { get; set; }

        //////////////////////////////////////////////////

        public String OutputVariableName { get; set; }

        public String TargetSource { get; set; }

        public Boolean InArray { get; set; }

        public Object Result { get; set; }

        //////////////////////////////////////////////////

        public S4JToken this[int index]
        {
            get { return this.Children[index]; }
            set { Children[index] = value; }
        }

        //////////////////////////////////////////////////

        public bool IsObjectKey { get; set; }

        public bool IsObjectValue { get; set; }

        public bool IsObjectSingleKey { get; set; }

        public bool IsCommited { get; set; }

        public bool WasRemoved { get; set; }

        //////////////////////////////////////////////////

        public S4JToken()
        {
            IsVisible = true;
            Tags = new Dictionary<string, object>();
        }

        //////////////////////////////////////////////////

        public IEnumerable<S4JToken> GetVisibleChildren()
        {
            foreach (S4JToken child in Children)
                if (child.IsVisible)
                    yield return child;
        }

        public S4JToken GetLastVisibleChild()
        {
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                S4JToken child = Children[i];
                if (child.IsVisible)
                    return child;
            }
            return null;
        }

        public S4JToken GetLastVisibleChild<T>() where T : class

        {
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                S4JToken child = Children[i];
                if (child is T && child.IsVisible)
                    return child;
            }
            return null;
        }

        public virtual Dictionary<String, Object> GetParameters()
        {
            return null;
        }

        public virtual void InsertChildToToken(Int32 Index, S4JToken Child)
        {
            S4JToken prevChild = Index > 0 ? Children[Index - 1] : null;
            S4JToken nextChild = Index + 1 < Children.Count ? Children[Index + 1] : null;

            Children.Insert(Index, Child);

            if (prevChild != null)
                prevChild.NextToken = Child;

            Child.PrevToken = prevChild;
            Child.NextToken = nextChild;

            if (nextChild != null)
                nextChild.PrevToken = Child;
        }

        public virtual void AddChildToToken(S4JToken Child)
        {
            S4JToken lastChild = Children.LastOrDefault();
            if (lastChild != null)
                lastChild.NextToken = Child;
            Child.PrevToken = lastChild;
            Children.Add(Child);
        }

        public virtual Int32 IndexOfChild(S4JToken OldChild)
        {
            Int32 childIndex = this.Children.IndexOf(OldChild);
            return childIndex;
        }

        public virtual bool RemoveChild(S4JToken OldChild, IList<S4JToken> NewChilds = null)
        {
            Int32 childIndex = IndexOfChild(OldChild);
            return RemoveChild(childIndex, NewChilds);
        }

        public virtual bool RemoveChild(Int32 Index, IList<S4JToken> NewChilds = null)
        {
            if (Index < 0)
                return false;

            S4JToken prevChild = Index > 0 ? Children[Index - 1] : null;
            S4JToken nextChild = Index + 1 < Children.Count ? Children[Index + 1] : null;

            var child = this.Children[Index];
            child.Parent = null;
            this.Children.RemoveAt(Index);

            if (prevChild != null)
                prevChild.NextToken = nextChild;

            if (nextChild != null)
                nextChild.PrevToken = prevChild;

            if (NewChilds != null)
                foreach (S4JToken newChild in NewChilds)
                {
                    InsertChildToToken(Index, newChild);
                    Index++;
                }

            return true;
        }

        public virtual void AppendCharsToToken(IList<Char> Chars)
        {
            S4JToken lastChild = this.Children.LastOrDefault();
            if (!(lastChild is S4JTokenTextValue) || lastChild.IsCommited)
            {
                lastChild = new S4JTokenTextValue();
                lastChild.Parent = this;
                AddChildToToken(lastChild);
            }
            lastChild.AppendCharsToToken(Chars);
        }

        public virtual void MarkAsObjectValue()
        {
            this.IsObjectValue = true;
            this.IsObjectSingleKey = false;
            this.IsObjectKey = false;
        }

        public virtual void MarkLastChildAsObjectKey()
        {
            S4JToken lastChild = this.Children.LastOrDefault();
            if (lastChild == null)
                return;

            lastChild.MarkAsObjectKey();
        }

        public virtual void MarkAsObjectKey()
        {
            this.IsObjectKey = true;
            this.IsObjectSingleKey = false;
        }

        public virtual void MarkAsSingleObjectKey()
        {
            this.IsObjectKey = false;
            this.IsObjectSingleKey = true;
        }

        public virtual bool Commit()
        {
            // IsCommited = true;

            S4JToken lastChild = null;
            while (true)
            {
                lastChild = this.Children.LastOrDefault();
                if (lastChild is S4JTokenTextValue txtVal)
                {
                    if (txtVal.Commit())
                        break;
                }
                else
                {
                    break;
                }
            }

            CalculateIsSingleKey(this, lastChild);

            for (var i = 0; i < this.Children.Count; i++)
            {
                S4JToken token = this.Children[i];
                if (token?.State?.StateType != EStateType.S4J_TAG)
                    continue;

                if (token.NextToken != null)
                {
                    foreach (var tagKV in token.Tags)
                        token.NextToken.Tags[tagKV.Key] = tagKV.Value;
                }

                RemoveChild(i);
                i--;
            }


            //CalculateIsSingleKey(this, lastChild);
            // CalculateIsSingleKey(this.Parent, this);

            return true;
        }

        private void CalculateIsSingleKey(S4JToken ParentToken, S4JToken ChildToken)
        {
            if (ChildToken == null)
                return;

            // ustalenie IsSingleKey = true
            // próba określenia czy token jest w obiekcie
            // oraz czy jest 'kluczem bez wartosci' 
            if (ParentToken is S4JTokenObject ||
                ParentToken is S4JTokenParameters ||
                ParentToken is S4JTokenTag)
            {
                S4JToken prevChild = null;
                foreach (S4JToken child in ParentToken.Children)
                {
                    if (!child.IsObjectKey)
                    {
                        if (prevChild == null ||
                            prevChild.IsObjectKey == false)
                        {
                            child.MarkAsSingleObjectKey();
                        }

                        else if (prevChild != null &&
                                 prevChild.IsObjectKey == true)
                        {
                            child.MarkAsObjectValue();
                        }
                    }

                    prevChild = child;
                }
            }
        }

        public virtual string ToJson(Boolean Force = false)
        {
            StringBuilder builder = new StringBuilder();
            BuildJson(builder, Force);
            return builder.ToString();
        }

        public virtual bool BuildJson(StringBuilder Builder, Boolean Force)
        {
            if (!IsVisible && !Force)
                return false;

            ////////////////////////////////////

            if (State.FoundGates != null)
                foreach (var ch in State.FoundGates.First().Start)
                    Builder.Append(ch);
            else if (State.Gates.Count > 0)
                foreach (var ch in State.Gates[0].Start)
                    Builder.Append(ch);

            ////////////////////////////////////

            foreach (var child in Children)
                child.BuildJson(Builder, Force);

            ////////////////////////////////////

            if (State.FoundGates != null && !State.FoundGates.First().OmitEnd)
                foreach (var ch in State.FoundGates.First().End)
                    Builder.Append(ch);
            else if (State.Gates.Count > 0 && !State.Gates[0].OmitEnd)
                foreach (var ch in State.Gates[0].End)
                    Builder.Append(ch);

            return true;
        }

        public string ToJsonWithoutGate()
        {
            StringBuilder builder = new StringBuilder();
            BuildJson(builder, true);
            if (State?.FoundGates != null)
            {
                if (builder.ToString().StartsWith(new string(State.FoundGates.First().Start.ToArray())))
                {
                    builder.Remove(0, State.FoundGates.First().Start.Length);
                }
                if (!State.FoundGates.First().OmitEnd &&
                    builder.ToString().EndsWith(new string(State.FoundGates.First().End.ToArray())))
                {
                    builder.Remove(builder.Length - State.FoundGates.First().End.Length, State.FoundGates.First().End.Length);
                }
            }
            return builder.ToString();
        }

        public virtual S4JToken Clone()
        {
            S4JToken newToken = (S4JToken)this.MemberwiseClone();
            newToken.State = newToken.State?.Clone();
            newToken.Tags = new Dictionary<string, object>(this.Tags);
            newToken.Parent = null;
            newToken.PrevToken = null;
            newToken.NextToken = null;
            newToken.Children = newToken.Children.Select(i => { S4JToken token = i.Clone(); token.Parent = newToken; return token; }).ToList();
            // newToken.Parent = newToken.Parent?.Clone();
            return newToken;
        }

        public override string ToString()
        {
            return this.ToJson();
        }
    }

    public class S4JTokenStack : List<S4JToken>
    {
        public void Push(S4JToken Token)
        {
            this.Add(Token);
        }

        public void Pop()
        {
            this.RemoveAt(this.Count - 1);
        }

        public S4JToken Peek()
        {
            return this.LastOrDefault();
        }

        /*public S4JToken PeekNonValue()
        {
            return this.
                LastOrDefault(t => !t.State.IsValue); // && !t.State.IsComment);
        }*/
    }

    public class S4JFieldDescription
    {
        public String Name { get; set; }

        public S4JFieldType Type { get; set; }

        public S4JToken Token { get; set; }

        public Boolean IsRequired { get; set; }

        public static S4JFieldDescription Parse(String Name, String Type)
        {
            Type = (Type ?? "").Trim().ToLower();
            Name = (Name ?? "").Trim();
            if (string.IsNullOrEmpty(Name))
                throw new Exception("Field name should not be empty!");
            Boolean isRequired = Type.EndsWith("!");
            Type = Type.TrimEnd('!');
            S4JFieldType fieldType = (S4JFieldType)Enum.Parse(typeof(S4JFieldType), Type, true);
            return new S4JFieldDescription()
            {
                Name = Name,
                IsRequired = isRequired,
                Type = fieldType
            };
        }

        public static S4JFieldDescription Parse(String Name, S4JToken Token)
        {
            Name = (Name ?? "").Trim();
            if (string.IsNullOrEmpty(Name))
                throw new Exception("Field name should not be empty!");

            return new S4JFieldDescription()
            {
                Name = Name,
                IsRequired = false,
                Type = S4JFieldType.TOKEN,
                Token = Token
            };
        }

        internal object ToJson()
        {
            return Type.ToString().ToLower();
        }
    }

    [Serializable]
    public class S4JNullParameterException : Exception
    {
        public S4JNullParameterException() { }
        public S4JNullParameterException(string message) : base(message) { }
        public S4JNullParameterException(string message, Exception inner) : base(message, inner) { }
        protected S4JNullParameterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class S4JInvalidParameterTypeException : Exception
    {
        public S4JInvalidParameterTypeException() { }
        public S4JInvalidParameterTypeException(string message) : base(message) { }
        public S4JInvalidParameterTypeException(string message, Exception inner) : base(message, inner) { }
        protected S4JInvalidParameterTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public enum S4JFieldType
    {
        ANY,
        TOKEN,
        STRING,
        //TIMESPAN,
        INT,
        BOOL,
        //DOUBLE,
        FLOAT,
        DATETIME,
        ARRAY,
        OBJECT
    }
}
