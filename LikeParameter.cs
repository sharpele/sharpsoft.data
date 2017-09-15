using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class LikeParameter
    {
        public LikeParameter()
        {

        }
        public LikeParameter(string value, LikeMode mode = LikeMode.Contains)
        {
            Mode = mode;
            Value = value;
        }
        public LikeMode Mode { get; set; } = LikeMode.Contains;
        public string Value { get; set; }
    }
    public enum LikeMode
    {
        Contains,
        StartWith,
        EndWith
    }
}
