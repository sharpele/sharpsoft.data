using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data.Lexing
{
    public class Lexer
    {
        private LexerSetting _setting = LexerSetting.Default;
        private readonly string[] Operators = new string[] { "&&", "||", "==", "=", "!=", ">=", "<=", "<>", ">", "<", "+", "-", "*", "/", "%", "^", "~", "!", ".", "&", "|" };
        internal Lexer(string sou)
        {
            source = sou;
            cursor = 0;
            if (string.IsNullOrEmpty(source))
            {
                length = 0;
                eof = true;
            }
            length = source.Length;
        }
        public Lexer(string sou, LexerSetting setting) : this(sou)
        {
            _setting = setting;
        }
        /// <summary>
        /// 重置当前此法解析器的游标
        /// </summary>
        public void Reset()
        {
            cursor = 0;
        }
        private void Next(int offset = 1)
        {
            cursor += offset;
            if (cursor >= length)
            {
                eof = true;
            }
        }
        private char Preview(int offset)
        {
            if (cursor + offset >= length || cursor + offset < 0)
            {
                return '\0';
            }
            return source[cursor + offset];
        }
        private readonly string source;
        private int cursor = 0;//游标
        private readonly int length;//解析源的长度
        private bool eof = false;//是否已经到达源的末尾
        /// <summary>
        /// 获取当前位置的字符
        /// </summary>
        private char cur
        {
            get
            {
                if (eof)
                {
                    return '\0';
                }
                return source[cursor];
            }
        }
        /// <summary>
        /// 抛出异常
        /// </summary>
        /// <param name="msg"></param>
        private void ex(string msg)
        {
            throw new LexerException(msg) { Position = cursor };
        }
        /// <summary>
        /// 发出警告
        /// </summary>
        /// <param name="msg"></param>
        private void waring(string msg)
        {
            //预留
        }

        #region 字符断言
        /// <summary>
        /// 判断字符是否为表示名称标识的文字
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isLiteral(char c)
        {
            if (c == '_' || char.IsDigit(c) || char.IsLetter(c) || (c >= 0x4e00 && c <= 0x9fbb))//下划线、数字、字母、汉字
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 判断字符是否为表示名称标识的文字的有效首字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isLiteralFirst(char c)
        {
            if (c == '_' || char.IsLetter(c) || (c >= 0x4e00 && c <= 0x9fbb))//下划线、数字、字母、汉字
            {
                return true;
            }
            if (_setting.LiteralFirstChars != null)
            {
                //用户自定义的变量名首字母
                foreach (var item in
                _setting.LiteralFirstChars)
                {
                    if (c == item)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //是否是运算符
        private bool isOperator(char c)
        {
            switch (c)
            {//四则运算
                case '+':
                case '-':
                case '*':
                case '/':
                //逻辑运算
                case '|':
                case '&':
                //比较运算
                case '>':
                case '<':
                case '=':
                case '!':
                //其他
                case '%'://取模
                case '~'://按位取反
                case '^':
                case '.'://属性/函数读取
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        private void SkipWhiteSpace()
        {
            var c = cur;
            while (char.IsWhiteSpace(c) && !eof)
            {
                Next();
                c = cur;
            }
        }
        private Token token(TokenType type, string content)
        {
            return new Token(type, content) { Position = cursor };
        }
        public Token ReadToken()
        {
            SkipWhiteSpace();

            char c = cur;
            if (c == '(' || c == ')')
            {
                Next();
                return token(TokenType.Parenthese, c.ToString());
            }
            else if (c == '[' || c == ']')
            {
                Next();
                return token(TokenType.Bracket, c.ToString());
            }
            else if (c == '{' || c == '}')
            {
                Next();
                return token(TokenType.CurlyBracket, c.ToString());
            }
            else if (c == ',')
            {
                Next();
                return token(TokenType.Comma, c.ToString());
            }
            else if (c == ':')
            {
                Next();
                return token(TokenType.Colon, c.ToString());
            }
            else if (c == '?')
            {
                Next();
                return token(TokenType.Question, c.ToString());
            }
            else if (c == ';')
            {
                Next();
                return token(TokenType.Semicolon, c.ToString());
            }
            else if (c == _setting.StringSign)
            {
                return readString();
            }
            else if (c == '\0')
            {
                return token(TokenType.End, "\0");
            }
            else if (char.IsDigit(c))
            {//为数字
                return readNumeric();
            }
            else if (readKeyword() is Token tk)
            {
                return tk;
            }
            else if (readCustomOperator() is Token t)
            {
                return t;
            }
            else if (isLiteralFirst(c))
            {//为名称标识
                return readLiteral();
            }
            else if (isOperator(c))
            {
                return readOperator();
            }
            else
            {
                var com = readComments();
                if (com != null)
                {
                    return com;
                }
                ex("语法错误，在当前位置不可识别的字符：[" + c + "]");
            }

            return null;
        }
        /// <summary>
        /// 解析当前源
        /// </summary>
        /// <returns></returns>
        public Token[] Reslove()
        {
            this.Reset();
            List<Token> l = new List<Token>();
            Token t = null;
            do
            {
                t = this.ReadToken();
                l.Add(t);
            } while (t != null && t.Type != TokenType.End);
            this.Reset();
            return l.ToArray();
        }

        private Token readComments()
        {
            var com = _setting.OutlineCommentsSign;
            StringBuilder sb = new StringBuilder(20);
            if (matchText(com))
            {//注释开始
                Next(com.Length);
                var c = cur;
                while (c != '\n' && !eof)
                {
                    sb.Append(c);
                    Next();
                    c = cur;
                }
                return token(TokenType.Comments, sb.ToString());
            }
            if (matchText(_setting.InlineCommentsStartSign))
            {//行内注释开始
                if (string.IsNullOrEmpty(_setting.InlineCommentsEndSign))
                {//未指定行内注释的结束符号，该设置无效。
                    waring("未指定行内注释的结束符号，该设置无效。");
                    return null;
                }
                Next(_setting.InlineCommentsStartSign.Length);
                var endchar = _setting.InlineCommentsEndSign[0];
                var c = cur;
                while (!eof)
                {
                    if (c == endchar)
                    {
                        if (matchText(_setting.InlineCommentsEndSign))
                        {//行内注释结束
                            Next(_setting.InlineCommentsEndSign.Length);
                            return token(TokenType.Comments, sb.ToString());
                        }
                    }
                    sb.Append(c);
                    Next();
                    c = cur;
                }
                //直到结尾行内注释都没结束。
                ex("直到文档结尾，未发现行内注释的结束符号：[" + _setting.InlineCommentsEndSign + "]");
            }
            return null;
        }
        /// <summary>
        /// 尝试从当前位置匹配一段固定的字符串
        /// </summary>
        /// <returns></returns>
        private bool matchText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            for (int i = 0; i < text.Length; i++)
            {

                if (!charEquls(Preview(i), text[i]))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 根据给定的设置比较两个字符
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        private bool charEquls(char c1, char c2)
        {
            if (_setting.IgnoreCase)
            {//忽略大小写
                return string.Compare(c1.ToString(), c2.ToString(), true) == 0;
            }
            else
            {
                return c1 == c2;
            }
        }
        /// <summary>
        /// 读取一个符号符号表示的运算符
        /// </summary>
        /// <returns></returns>
        private Token readOperator()
        {
            StringBuilder sb = new StringBuilder(2);
            var c = cur;
            if (Operators != null)
            {
                foreach (var item in Operators)
                {
                    if (matchText(item))
                    {
                        Next(item.Length);
                        return token(TokenType.Operator, item);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 从当前位置获取一个自定义的操作符，不存在则返回null。
        /// </summary>
        /// <returns></returns>
        private Token readCustomOperator()
        {
            StringBuilder sb = new StringBuilder(6);
            if (_setting.CustomOperators != null)
            {
                foreach (var item in _setting.CustomOperators)
                {
                    if (matchText(item) && !isLiteral(Preview(item.Length)))
                    {
                        Next(item.Length);
                        return token(TokenType.Operator, item);
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 读取一个关键字
        /// </summary>
        /// <returns></returns>
        private Token readKeyword()
        {
            StringBuilder sb = new StringBuilder(6);
            if (_setting.Keywords != null)
            {
                foreach (var item in _setting.Keywords)
                {
                    if (matchText(item) && !isLiteral(Preview(item.Length)))
                    {//完整匹配关键字并且关键字不是其他标识符的一部分
                        Next(item.Length);
                        return token(TokenType.Keyword, item);
                    }
                }
            }
            return null;
        }
        //读取一个数字
        private Token readNumeric()
        {
            StringBuilder sb = new StringBuilder(5);
            var c = cur;
            bool dotexists = false;//是否已经存在小数点
            while (char.IsDigit(c) || c == '.')
            {
                if (c == '.')
                {
                    if (dotexists)
                    {
                        ex("尝试读取一串数字，但是似乎出现了多个小数点。");
                    }
                    dotexists = true;
                }
                sb.Append(c);
                Next();
                c = cur;
            }
            return token(TokenType.Numeric, sb.ToString());
        }
        /// <summary>
        /// 读取文字，包括关键字、表名、列名、函数名等。
        /// </summary>
        /// <returns></returns>
        private Token readLiteral()
        {
            StringBuilder sb = new StringBuilder(10);
            var c = cur;
            while (isLiteral(c) || isLiteralFirst(c))
            {
                sb.Append(c);
                Next();
                c = cur;
            }
            return token(TokenType.Literal, sb.ToString());
        }
        /// <summary>
        /// 读取一段字符串
        /// </summary>
        /// <returns></returns>
        private Token readString()
        {
            StringBuilder sb = new StringBuilder(10);
            Next();//跳过字符串的开头标识
            char c = cur;
            while (!eof)
            {
                c = cur;
                if (c == _setting.StringSign)
                {//字符串内遇到字符串标识
                    if (_setting.TransferredType == TransferredType.DoubleSign && Preview(1) == _setting.StringSign)
                    {//双标识转义
                     //转义，忽略双标识。
                        sb.Append(_setting.StringSign);
                        Next(2);
                        continue;
                    }
                    else
                    {//结束字符串
                        Next();
                        goto end;
                    }
                }
                else if (c == '\\' && _setting.TransferredType == TransferredType.UseBackslash && Preview(1) == _setting.StringSign)//反斜杠转义
                {//忽略反斜杠
                    sb.Append(_setting.StringSign);
                    Next(2);
                    continue;
                }
                sb.Append(c);
                Next();
            }
            throw new Exception("读取一段字符串时发生异常：未发现字符串结束标识。");
            end:
            return token(TokenType.String, sb.ToString());
        }

    }
}
