using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SharpSoft.Data.Lexing;

namespace SharpSoft.Data
{
    public abstract class ParserBase
    {
        protected Token[] tokens;
        private int cursor = 0;
        private int length;
        private ParserContext Context;
        /// <summary>
        /// 转到上一级上下文
        /// </summary>
        protected void ContextUp()
        {
            //if (Context != null)
            //{
            //    SetCursor(Context.EndIndex + 1);
            //}
            Context = Context?.Parent;
        }
        /// <summary>
        /// 转到下一级上下文
        /// </summary>
        protected void ContextIn(int start, int end)
        {
            if (Context != null && end == -1)
            {//下级的endindex继承上级
                end = Context.EndIndex;
            }
            var con = new ParserContext() { StratIndex = start, EndIndex = end, Parent = Context };
            Context = con;
        }
        protected ParserBase()
        {

        }
        public ParserBase(Lexer lexer)
        {
            SetTokens(lexer.Reslove());
        }
        public ParserBase(Token[] ts)
        {
            SetTokens(ts);
        }
        protected bool matchToken(Token t, TokenType type, string content = null)
        {
            if (t == null)
            {
                return false;
            }
            return t.Type == type && (content == null || t.Content == content);
        }
        protected void SetTokens(Token[] ts)
        {
            tokens = ts;
            length = ts.Length;
        }
        /// <summary>
        /// 跳过注释
        /// </summary>
        protected void SkipComments()
        {
            while (true)
            {
                var t = cur;
                if (t != null && t.Type == TokenType.Comments)
                {
                    Next();
                }
                else
                {
                    return;
                }
            }
        }
        public void Reset()
        {

            cursor = 0;
        }
        /// <summary>
        /// 将游标置于指定位置
        /// </summary>
        /// <param name="pos"></param>
        protected void SetCursor(int pos)
        {
            cursor = pos;
        }
        /// <summary>
        /// 获取游标位置
        /// </summary>
        /// <returns></returns>
        protected int GetCursor()
        {
            return cursor;
        }
        protected Token Next(int offset = 1)
        {
            cursor += offset;
            return cur;
        }
        protected Token Preview(int offset)
        {
            if (cursor + offset < 0 || cursor + offset >= length)
            {
                return null;
            }
            return tokens[cursor + offset];
        }
        protected Token cur
        {
            get
            {
                if (length <= 0)
                {
                    return null;
                }
                if (cursor < 0)
                {
                    return null;
                }
                if (Context == null)
                {
                    if (cursor >= length)
                    {
                        return null;
                    }
                }
                else
                {
                    if (cursor < Context.StratIndex || (Context.EndIndex != -1 && cursor > Context.EndIndex))
                    {
                        return null;
                    }
                }

                return tokens[cursor];
            }
        }
        [DebuggerStepThrough]
        protected void ex(string msg)
        {
            throw new ParserException(msg) { Token = cur };
        }
        [DebuggerStepThrough]
        protected void waring(string msg)
        {
            //预留
        }
        /// <summary>
        /// 从当前位置读取一对括号内的内容
        /// </summary>
        /// <param name="start">输出开括号位置</param>
        /// <param name="end">输出闭括号位置</param>
        protected void ReadTokensInBrackets(out int start, out int end)
        {
            start = end = 0;
            Token t = cur;
            if ((t.Type == TokenType.Parenthese || t.Type == TokenType.Bracket || t.Type == TokenType.CurlyBracket) && (t.Content == "[" || t.Content == "(" || t.Content == "{"))
            {
                TokenType type = t.Type;
                int leftcount = 0;//记录左括号的数量
                start = cursor;
                for (int i = cursor; i < length; i++)
                {
                    Token nt = tokens[i];
                    char left, right;
                    getBracketsRight(type, out left, out right);
                    if (nt.Type == type && nt.Content == left.ToString())
                    {
                        leftcount++;
                    }
                    else if (nt.Type == type && nt.Content == right.ToString())
                    {
                        leftcount--;
                    }

                    if (leftcount == 0)
                    {//括号已闭合
                        end = i;
                        return;
                    }

                }
                ex("未找到匹配的右括号。");

            }
            else
            {//当前位置不是左括号
                ex("当前位置不是左括号。");
            }
        }
        /// <summary>
        /// 获取括号标记的右括号字符
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private void getBracketsRight(TokenType t, out char left, out char right)
        {
            switch (t)
            {
                case TokenType.Parenthese:
                    left = '(';
                    right = ')';
                    break;
                case TokenType.Bracket:
                    left = '[';
                    right = ']';
                    break;
                case TokenType.CurlyBracket:
                    left = '{';
                    right = '}';
                    break;
                default:
                    throw new Exception("非括号标记。");
            }
        }

    }
}
