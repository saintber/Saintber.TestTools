namespace Saintber.TestTools.UnitTests;

[TestClass]
public sealed class AssertExtensionsTests
{
    #region == 必填檢核 ==
    [TestMethod()]
    public async Task HasRequiredFieldsAsyncTest()
    {
        // 排列

        // 判斷提示
        // -- 未輸入檢核欄位則直接檢核成功，否則異常
        await TestExtensions.ThrowAssertAsync(
            () => ExtendedAssert.HasRequiredFieldsAsync(() => FullModel, ThrowIfNameEmpty)
            , ex => "未輸入檢核欄位應直接通過，卻拋出例外狀況");

        // -- 輸入有正確檢核(拋出異常)欄位，應檢核成功
        await TestExtensions.ThrowAssertAsync(() => ExtendedAssert.HasRequiredFieldsAsync(() => FullModel, ThrowIfNameEmpty, null, nameof(TestModel.Name))
            , ex => $"輸入檢核欄位 {nameof(TestModel.Name)} 必填，應通過卻拋出例外狀況");

        // -- 輸入未正確檢核(無異常)欄位，應檢核失敗
        await ExtendedAssert.ThrowsAsync(() => ExtendedAssert.HasRequiredFieldsAsync(() => FullModel, ThrowIfNameEmpty, null, nameof(TestModel.Id))
            , ex => $"輸入檢核欄位 {nameof(TestModel.Id)} 必填，應失敗卻未拋出例外狀況");
    }

    Func<TestModel, Task> ThrowIfNameEmpty => m =>
    {
        if (string.IsNullOrEmpty(m.Name))
            throw new ArgumentNullException(nameof(TestModel.Name));
        return Task.CompletedTask;
    };

    TestModel FullModel => new TestModel { Id = 1, Name = "測試" };
    #endregion

    #region == 值檢核 ==
    [TestMethod()]
    public void AreEqualTest()
    {
        // 排列
        int[] source = [1, 2, 3];
        int[] sameTarget = [1, 2, 3];
        int[] notSameTarget = [1, 4, 3];
        int[] lessTarget = [1, 2];
        int[] moreTarget = [1, 2, 3, 4];

        int[] source2 = [4, 5, 6];
        int[] sameTarget2 = [4, 5, 6];

        int[][] sourceArray = [source, source2];
        int[][] sameTargetArray = [sameTarget, sameTarget2];
        int[][] notSameTargetArray = [notSameTarget, sameTarget2];
        int[][] lessTargetArray = [sameTarget];
        int[][] moreTargetArray = [sameTarget, sameTarget2, sameTarget2];
        int[][] lessElementTargetArray = [lessTarget, sameTarget2];
        int[][] moreElementTargetArray = [moreTarget, sameTarget2];


        // 作用
        TestExtensions.ThrowAssert(() => ExtendedAssert.AreEqual(source, sameTarget)
            , ex => { return $"應相等卻拋出異常訊息：{ex.Message}"; });
        Assert.Throws<Exception>(() => ExtendedAssert.AreEqual(source, notSameTarget), ex => "不相等目標集合未拋出檢核失敗");
        Assert.Throws<Exception>(() => ExtendedAssert.AreEqual(source, lessTarget), ex => "較小目標集合未拋出檢核失敗");
        Assert.Throws<Exception>(() => ExtendedAssert.AreEqual(source, moreTarget), ex => "較大目標集合未拋出檢核失敗");

        TestExtensions.ThrowAssert(() => ExtendedAssert.AreEqual(sourceArray, sameTargetArray)
            , ex => { return $"陣列集合應相等卻拋出異常訊息：{ex.Message}"; });
        Assert.Throws<Exception>(() => ExtendedAssert.AreEqual(sourceArray, notSameTargetArray), ex => "不相等目標陣列集合未拋出檢核失敗");
        Assert.Throws<Exception>(() => ExtendedAssert.AreEqual(sourceArray, lessTargetArray), ex => "較小目標陣列集合未拋出檢核失敗");
        Assert.Throws<Exception>(() => ExtendedAssert.AreEqual(sourceArray, moreTargetArray), ex => "較大目標陣列集合未拋出檢核失敗");
        Assert.Throws<Exception>(() => ExtendedAssert.AreEqual(sourceArray, lessElementTargetArray), ex => "目標陣列集合缺少元素未拋出檢核失敗");
        Assert.Throws<Exception>(() => ExtendedAssert.AreEqual(sourceArray, moreElementTargetArray), ex => "目標陣列集合多出元素未拋出檢核失敗");
    }
    #endregion

    #region == 篩選條件檢核 ==
    [TestMethod]
    public async Task FilterAppliedAsyncTest()
    {
        // 排列
        var correctFilterHandler = () => new TestModel
        {
            Id = 1,
            Name = "test",
        };
        var wrongFilterHandler = () => new TestModel
        {
            Id = 2,
            Name = "test2",
        };
        var singleModels = new List<TestModel> { new TestModel { Id = 1, Name = "test" } };
        var multiModels = new List<TestModel> { new TestModel { Id = 1, Name = "test" }, new TestModel { Id = 2, Name = "test2" } };
        var correctGetHandler = (TestModel filter) =>
        {
            IEnumerable<TestModel> models = singleModels.AsEnumerable();
            if (filter.Id.HasValue) models = models.Where(x => x.Id == filter.Id);
            if (!string.IsNullOrEmpty(filter.Name)) models = models.Where(x => x.Name == filter.Name);
            return Task.FromResult(models);
        };
        var moreThenOneGetHandler = (TestModel filter) =>
        {
            IEnumerable<TestModel> models = multiModels.AsEnumerable();
            if (!filter.Id.HasValue) models = models.Where(x => x.Id == filter.Id);
            if (!string.IsNullOrEmpty(filter.Name)) models = models.Where(x => x.Name == filter.Name);
            return Task.FromResult(models);
        };
        var lostFieldGetHandler = (TestModel filter) =>
        {
            IEnumerable<TestModel> models = singleModels.AsEnumerable();
            if (!string.IsNullOrEmpty(filter.Name)) models = models.Where(x => x.Name == filter.Name);
            return Task.FromResult(models);
        };
        var wrongFieldGetHandler = (TestModel filter) =>
        {
            IEnumerable<TestModel> models = singleModels.AsEnumerable();
            if (!filter.Id.HasValue) models = models.Where(x => x.Id == filter.Id + 1);
            if (!string.IsNullOrEmpty(filter.Name)) models = models.Where(x => x.Name == filter.Name);
            return Task.FromResult(models);
        };

        // 作用
        await TestExtensions.ThrowAssertAsync(() => ExtendedAssert.FilterAppliedAsync(correctFilterHandler, wrongFilterHandler, correctGetHandler)
            , ex => $"全正確檢核失敗，拋出異常：{ex.Message}");
        await ExtendedAssert.ThrowsAsync(() => ExtendedAssert.FilterAppliedAsync(correctFilterHandler, wrongFilterHandler, moreThenOneGetHandler)
            , ex => $"資料筆數不為單筆仍通過檢核");
        await ExtendedAssert.ThrowsAsync(() => ExtendedAssert.FilterAppliedAsync(correctFilterHandler, wrongFilterHandler, lostFieldGetHandler)
            , ex => $"缺少篩選條件仍通過檢核");
        await ExtendedAssert.ThrowsAsync(() => ExtendedAssert.FilterAppliedAsync(correctFilterHandler, wrongFilterHandler, wrongFieldGetHandler)
            , ex => $"未查到資料檢仍通過檢核");
    }
    #endregion

    public class TestModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
}
