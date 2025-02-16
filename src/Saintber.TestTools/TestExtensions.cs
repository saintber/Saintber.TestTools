using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Saintber.TestTools
{
    public static class TestExtensions
    {
        #region == 比較函式 ==
        /// <summary>
        /// 比較兩個字串集合清單是否相等。
        /// </summary>
        /// <param name="source">來源字串集合清單。</param>
        /// <param name="target">目標字串集合清單。</param>
        /// <returns>比對結果。</returns>
        public static bool IsEqual<T>(this IEnumerable<IEnumerable<T>> source, IEnumerable<IEnumerable<T>> target)
        {
            if (source.Count() != target.Count())
            {
                return false;
            }

            for (int i = 0; i < source.Count(); i++)
            {
                if (!source.ElementAt(i).IsEqual(target.ElementAt(i)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 比較兩個字串集合是否相等。
        /// </summary>
        /// <param name="source">來源字串集合。</param>
        /// <param name="target">目標字串集合。</param>
        /// <returns>比對結果。</returns>
        public static bool IsEqual<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            if (source.Count() != target.Count())
            {
                return false;
            }

            for (int i = 0; i < source.Count(); i++)
            {
                if (source.ElementAt(i)?.Equals(target.ElementAt(i)) != true)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region == 例外狀況 ==
        /// <summary>
        /// 重拋例外狀況為判斷提示例外狀況。
        /// </summary>
        /// <param name="action">執行函式。</param>
        /// <param name="messageBuilder">判斷提示訊息建構函式。</param>
        public static void ThrowAssert(this Action action, Func<Exception, string> messageBuilder)
            => action.ThrowAssert(ex => new AssertFailedException(messageBuilder(ex)));

        /// <summary>
        /// 重拋例外狀況為判斷提示例外狀況。
        /// </summary>
        /// <param name="action">執行函式。</param>
        /// <param name="exceptionBuilder">例外狀況建構函式。</param>
        public static void ThrowAssert(this Action action, Func<Exception, AssertFailedException> exceptionBuilder)
            => action.ThrowAssert<AssertFailedException>(exceptionBuilder);

        /// <summary>
        /// 重拋例外狀況為判斷提示例外狀況。
        /// </summary>
        /// <typeparam name="TException">回拋例外狀況型別。</typeparam>
        /// <param name="action">執行函式。</param>
        /// <param name="exceptionBuilder">例外狀況建構函式。</param>
        public static void ThrowAssert<TException>(this Action action, Func<Exception, TException> exceptionBuilder)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw exceptionBuilder(ex);
            }
        }


        /// <summary>
        /// 重拋例外狀況為判斷提示例外狀況。
        /// </summary>
        /// <param name="actionAsync">執行函式。</param>
        /// <param name="messageBuilder">判斷提示訊息建構函式。</param>
        /// <returns>非同步作業。</returns>
        public static Task ThrowAssertAsync(this Func<Task> actionAsync, Func<Exception, string> messageBuilder)
            => actionAsync.ThrowAssertAsync(ex => new AssertFailedException(messageBuilder(ex)));

        /// <summary>
        /// 重拋例外狀況為判斷提示例外狀況。
        /// </summary>
        /// <param name="actionAsync">執行函式。</param>
        /// <param name="exceptionBuilder">例外狀況建構函式。</param>
        /// <returns>非同步作業。</returns>
        public static Task ThrowAssertAsync(this Func<Task> actionAsync, Func<Exception, AssertFailedException> exceptionBuilder)
            => actionAsync.ThrowAssertAsync<AssertFailedException>(exceptionBuilder);

        /// <summary>
        /// 重拋例外狀況為判斷提示例外狀況。
        /// </summary>
        /// <typeparam name="TException">回拋例外狀況型別。</typeparam>
        /// <param name="actionAsync">執行函式。</param>
        /// <param name="exceptionBuilder">例外狀況建構函式。</param>
        /// <returns>非同步作業。</returns>
        public static async Task ThrowAssertAsync<TException>(this Func<Task> actionAsync, Func<Exception, TException> exceptionBuilder)
            where TException : Exception
        {
            try
            {
                await actionAsync();
            }
            catch (Exception ex)
            {
                throw exceptionBuilder(ex);
            }
        }
        #endregion
    }
}
