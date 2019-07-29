using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FlareTables
{
    [SuppressMessage("ReSharper", "MemberCanBeInternal")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class PageStateHandler
    {
        private readonly Shared.StateHasChanged _stateUpdater;
        private          int                    _current;

        private int _pageSize;
        private int _rowCount;

        internal PageStateHandler(int pageSize, Shared.StateHasChanged stateUpdater)
        {
            Current       = 0;
            PageSize      = pageSize;
            _stateUpdater = stateUpdater;
        }

        private int NumPages => (int) Math.Ceiling(_rowCount / (decimal) _pageSize);

        public   bool CanNext => Current + 1 < NumPages;
        public   bool CanPrev => Current - 1 >= 0;
        internal int  Skip    => _pageSize * Current;

        public int Current
        {
            get => _current;
            private set
            {
                _current = value;
                _stateUpdater?.Invoke();
            }
        }

        internal int RowCount
        {
            set
            {
                _rowCount = value;
                ResetCurrentPage();
            }
        }

        internal int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                ResetCurrentPage();
            }
        }

        public string Info =>
            $"Showing {Skip + 1} to {Math.Min(Skip + _pageSize, _rowCount)} of {_rowCount:#,##0} | {NumPages} pages";

        private void ResetCurrentPage()
        {
            if (Current < NumPages || NumPages == 0) return;
            Current = NumPages - 1;
        }

        public void Next()
        {
            Current++;
        }

        public void Previous()
        {
            Current--;
        }

        public void First()
        {
            Current = 0;
        }

        public void Last()
        {
            Current = NumPages - 1;
        }

        public void Jump(int page)
        {
            Current = page;
        }

        // Algorithm in part by @dunika from:
        // https://gist.github.com/kottenator/9d936eb3e4e3c3e02598#gistcomment-2607388
        public IEnumerable<int> Pages()
        {
            const int     listLength = 7;
            const decimal offset     = (decimal) (listLength / 2.0);

            decimal start = Current - Math.Floor(offset);
            decimal end   = Current + Math.Ceiling(offset);

            if (NumPages <= listLength)
            {
                start = 0;
                end   = NumPages;
            }
            else if (Current <= offset)
            {
                start = 0;
                end   = listLength;
            }
            else if (Current + offset >= NumPages)
            {
                start = NumPages - listLength;
                end   = NumPages;
            }

            List<int> range = Enumerable.Range((int) start, (int) (end - start)).ToList();

            if (start == 1)
            {
//                range.RemoveAt(range.Count - 1);
                range.Insert(0, 0);
            }
            else if (start > 0)
            {
//                range = range.Skip(1).ToList();
//                range.RemoveAt(range.Count - 1);
                range.Insert(0, -1);
                range.Insert(0, 0);
            }

            if (end == NumPages - 1)
            {
                range.Add(NumPages - 1);
            }
            else if (end < NumPages)
            {
                range.Add(-1);
                range.Add(NumPages - 1);
            }

            return range;
        }
    }
}