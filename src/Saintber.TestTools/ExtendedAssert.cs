using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Saintber.TestTools;

/// <summary>
/// 判斷提示擴充函式庫。
/// </summary>
public static class ExtendedAssert
{
    #region == 必填檢核 ==
    /// <summary>
    /// 檢核必填欄位，依序設定指定的欄位為 Null 值並執行指定的函式，若函式未拋回例外狀況則檢核失敗。
    /// </summary>
    /// <typeparam name="T">待檢核模型型別。</typeparam>
    /// <param name="getInitModelFunction">取得預設模型的函式。</param>
    /// <param name="action">待檢核的函式。</param>
    /// <param name="message">檢核失敗訊息。</param>
    /// <param name="fields">待檢核欄位。</param>
    /// <returns>非同步作業。 </returns>
    public static Task HasRequiredFieldsAsync<T>(this Func<T> getInitModelFunction
        , Func<T, Task> action, string? message = null, params string[] fields)
        => getInitModelFunction.HasRequiredFieldsAsync<T, Exception>(action, message, fields);

    /// <summary>
    /// 檢核必填欄位，依序設定指定的欄位為 Null 值並執行指定的函式，若函式未拋回例外狀況則檢核失敗。
    /// </summary>
    /// <typeparam name="T">待檢核模型型別。</typeparam>
    /// <typeparam name="TException">例外狀況型別。</typeparam>
    /// <param name="getInitModelFunction">取得預設模型的函式。</param>
    /// <param name="action">待檢核的函式。</param>
    /// <param name="message">檢核失敗訊息。</param>
    /// <param name="fields">待檢核欄位。</param>
    /// <returns>非同步作業。 </returns>
    public static Task HasRequiredFieldsAsync<T, TException>(this Func<T> getInitModelFunction
        , Func<T, Task> action, string? message = null, params string[] fields)
        where TException : Exception
        => getInitModelFunction.HasRequiredFieldsAsync<T, TException>(action, m => message ?? string.Empty, fields);

    /// <summary>
    /// 檢核必填欄位，依序設定指定的欄位為 Null 值並執行指定的函式，若函式未拋回例外狀況則檢核失敗。
    /// </summary>
    /// <typeparam name="T">待檢核模型型別。</typeparam>
    /// <typeparam name="TException">例外狀況型別。</typeparam>
    /// <param name="getInitModelFunction">取得預設模型的函式。</param>
    /// <param name="action">待檢核的函式。</param>
    /// <param name="messageHandler">檢核失敗訊息取得函式，第一欄為檢核失敗的欄位。</param>
    /// <param name="fields">待檢核欄位。</param>
    /// <returns>非同步作業。 </returns>
    public static async Task HasRequiredFieldsAsync<T, TException>(this Func<T> getInitModelFunction
        , Func<T, Task> action, Func<string, string> messageHandler, params string[] fields)
        where TException : Exception
    {
        foreach (var field in fields)
        {
            var property = typeof(T).GetProperty(field);
            var value = getInitModelFunction();
            property?.SetValue(value, null);
            await Assert.ThrowsAsync<TException>(() => action(value), messageHandler(field)).ConfigureAwait(false);
        }
    }
    #endregion

    #region == 篩選條件檢核 ==
    /// <summary>
    /// 篩選條件檢核。
    /// </summary>
    /// <typeparam name="T">查詢結果型別。</typeparam>
    /// <typeparam name="TFilter">篩選條件型別。</typeparam>
    /// <param name="getCorrectFilterHandler">取得篩選條件全欄位正確處理函式。</param>
    /// <param name="getErrorFilterlHandler">取得篩選條件全欄位錯誤處理函式。</param>
    /// <param name="getHandler">取得查詢結果處理函式。</param>
    /// <returns>非同步作業。</returns>
    /// <exception cref="AssertFailedException">失敗斷言例外狀況。</exception>
    public static async Task FilterAppliedAsync<T, TFilter>(
        this Func<TFilter> getCorrectFilterHandler
        , Func<TFilter> getErrorFilterlHandler
        , Func<TFilter, Task<IEnumerable<T>>> getHandler
        , int TestDataCount = 1)
        where TFilter : new()
    {
        // 無輸入條件
        var filter = new TFilter();
        var response = await getHandler(filter);
        if ((response?.Count() ?? 0) != TestDataCount) throw new AssertFailedException($"無篩選欄位篩選結果筆數不為 {TestDataCount}");

        // 取得欄位
        var filterProperties = typeof(TFilter).GetProperties();
        var modelProperties = typeof(T).GetProperties();

        // 欄位查詢檢核
        foreach (var property in filterProperties)
        {
            // 錯誤篩選條件檢核
            filter = new TFilter();
            var wrongFilter = getErrorFilterlHandler();
            var value = property.GetValue(wrongFilter);
            property.SetValue(filter, value);
            response = await getHandler(filter);
            if (response.Any()) throw new AssertFailedException($"欄位 {property.Name}、資料值 {value} 篩選結果應為 0 筆");
        }

        // 欄位全代入查詢檢核
        var correctFilter = getCorrectFilterHandler();
        response = await getHandler(correctFilter);
        if ((response?.Count() ?? 0) < 1) throw new AssertFailedException("代入全正確欄位無法篩選到結果");
    }
    #endregion

    #region == 值檢核 ==
    /// <summary>
    /// 測試兩個元素清單是否相等。
    /// </summary>
    /// <typeparam name="T">元素型別。</typeparam>
    /// <param name="source">來源元素集合清單。</param>
    /// <param name="target">目標元素集合清單。</param>
    /// <returns>比對結果。</returns>
    public static void AreEqual<T>(this IEnumerable<IList<T>> source, IEnumerable<T[]> target)
        => source.Select(x => x.AsEnumerable()).AreEqual(target);

    /// <summary>
    /// 測試兩個元素陣列清單是否相等。
    /// </summary>
    /// <typeparam name="T">元素型別。</typeparam>
    /// <param name="source">來源元素集合清單。</param>
    /// <param name="target">目標元素集合清單。</param>
    /// <returns>比對結果。</returns>
    public static void AreEqual<T>(this IEnumerable<T[]> source, IEnumerable<T[]> target)
        => source.Select(x => x.AsEnumerable()).AreEqual(target);

    /// <summary>
    /// 測試兩個元素集合清單是否相等。
    /// </summary>
    /// <typeparam name="T">元素型別。</typeparam>
    /// <param name="source">來源元素集合清單。</param>
    /// <param name="target">目標元素集合清單。</param>
    /// <returns>比對結果。</returns>
    public static void AreEqual<T>(this IEnumerable<IEnumerable<T>> source, IEnumerable<IEnumerable<T>> target)
        => source.AreEqual(target, null);

    /// <summary>
    /// 測試兩個元素集合清單是否相等。
    /// </summary>
    /// <typeparam name="T">元素型別。</typeparam>
    /// <param name="source">來源元素集合清單。</param>
    /// <param name="target">目標元素集合清單。</param>
    /// <param name="message">未擲回例外狀況時回傳的檢核失敗訊息。</param>
    /// <returns>比對結果。</returns>
    public static void AreEqual<T>(this IEnumerable<IEnumerable<T>> source, IEnumerable<IEnumerable<T>> target, string? message)
        => source.AreEqual(target, message, null);

    /// <summary>
    /// 測試兩個元素集合清單是否相等。
    /// </summary>
    /// <typeparam name="T">元素型別。</typeparam>
    /// <param name="source">來源元素集合清單。</param>
    /// <param name="target">目標元素集合清單。</param>
    /// <param name="message">未擲回例外狀況時回傳的檢核失敗訊息。</param>
    /// <param name="parameters">檢核失敗訊息參數。</param>
    /// <returns>比對結果。</returns>
    public static void AreEqual<T>(this IEnumerable<IEnumerable<T>> source, IEnumerable<IEnumerable<T>> target
        , string? message, params object?[]? parameters)
        => Assert.IsTrue(source.IsEqual(target), message, parameters);

    /// <summary>
    /// 測試兩個元素集合是否相等。
    /// </summary>
    /// <typeparam name="T">元素型別。</typeparam>
    /// <param name="source">來源元素集合。</param>
    /// <param name="target">目標元素集合。</param>
    /// <returns>比對結果。</returns>
    public static void AreEqual<T>(this IEnumerable<T> source, IEnumerable<T> target)
        => source.AreEqual(target, null);

    /// <summary>
    /// 測試兩個元素集合是否相等。
    /// </summary>
    /// <typeparam name="T">元素型別。</typeparam>
    /// <param name="source">來源元素集合。</param>
    /// <param name="target">目標元素集合。</param>
    /// <param name="message">未擲回例外狀況時回傳的檢核失敗訊息。</param>
    /// <returns>比對結果。</returns>
    public static void AreEqual<T>(this IEnumerable<T> source, IEnumerable<T> target, string? message)
        => source.AreEqual(target, message, null);

    /// <summary>
    /// 測試兩個元素集合是否相等。
    /// </summary>
    /// <typeparam name="T">元素型別。</typeparam>
    /// <param name="source">來源元素集合。</param>
    /// <param name="target">目標元素集合。</param>
    /// <param name="message">未擲回例外狀況時回傳的檢核失敗訊息。</param>
    /// <param name="parameters">檢核失敗訊息參數。</param>
    /// <returns>比對結果。</returns>
    public static void AreEqual<T>(this IEnumerable<T> source, IEnumerable<T> target
        , string? message, params object?[]? parameters)
        => Assert.IsTrue(source.IsEqual(target), message, parameters);
    #endregion

    #region == 簡化檢核 ==
    /// <summary>
    /// Asserts that the delegate <paramref name="action"/> throws an exception of type <see cref="Exception"/>
    /// and throws <see cref="AssertFailedException"/> if code does not throws exception.
    /// </summary>
    /// <param name="action">
    /// Delegate to code to be tested and which is expected to throw exception.
    /// </param>
    /// <param name="message">
    /// The message to include in the exception when <paramref name="action"/> does not throws exception.
    /// </param>
    /// <param name="messageArgs">
    /// An array of parameters to use when formatting <paramref name="message"/>.
    /// </param>
    /// <exception cref="AssertFailedException">
    /// Thrown if <paramref name="action"/> does not throws exception.
    /// </exception>
    /// <returns>
    /// The exception that was thrown.
    /// </returns>
    public static Task<Exception> ThrowsAsync(Func<Task> action, string message = "", params object[] messageArgs)
        => Assert.ThrowsAsync<Exception>(action, message, messageArgs);

    /// <summary>
    /// Asserts that the delegate <paramref name="action"/> throws an exception of type <see cref="Exception"/>
    /// and throws <see cref="AssertFailedException"/> if code does not throws exception.
    /// </summary>
    /// <param name="action">
    /// Delegate to code to be tested and which is expected to throw exception.
    /// </param>
    /// <param name="messageBuilder">
    /// A func that takes the thrown Exception (or null if the action didn't throw any exception) to construct the message to include in the exception when <paramref name="action"/> does not throws exception.
    /// </param>
    /// <exception cref="AssertFailedException">
    /// Thrown if <paramref name="action"/> does not throws exception.
    /// </exception>
    /// <returns>
    /// The exception that was thrown.
    /// </returns>
    public static Task<Exception> ThrowsAsync(Func<Task> action, Func<Exception?, string> messageBuilder)
        => Assert.ThrowsAsync<Exception>(action, messageBuilder);
    #endregion
}
