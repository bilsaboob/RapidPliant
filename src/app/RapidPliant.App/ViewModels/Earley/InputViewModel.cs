using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Earley
{
    public class InputViewModel : RapidViewModel
    {
        private string _input;

        public InputViewModel()
        {
        }

        public int Position { get { return get(() => Position); } set { set(() => Position, value); } }
        public int LineNo { get { return get(() => LineNo); } set { set(() => LineNo, value); } }
        public int ColNo { get { return get(() => ColNo); } set { set(() => ColNo, value); } }
        public bool IsAtEnd { get { return get(() => IsAtEnd); } set { set(() => IsAtEnd, value); } }

        public string Input { get { return get(() => Input); } set {set(()=>Input, value);} }

        public InputViewModel LoadForInput(string input)
        {
            _input = input;

            Input = input;
            IsAtEnd = false;
            LineNo = 1;
            ColNo = 1;

            if (_input == null || _input.Length == 0)
                IsAtEnd = true;

            return this;
        }

        public void Reset()
        {
            Position = 0;
            IsAtEnd = false;
            LineNo = 1;
            ColNo = 1;
        }

        public void MoveNext(int toPosition)
        {
            var count = toPosition - Position;
            if(count <= 0)
                return;

            var pos = Position;

            while (count > 0)
            {
                if (pos >= _input.Length)
                    return;
                
                var c = _input[pos++];
                Position = pos;
                ColNo++;

                if (c == '\n')
                {
                    LineNo++;
                    ColNo = 1;
                }

                count--;
            }
        }
    }
}
