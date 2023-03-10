namespace BugsFarm.UnitSystem
{
    public interface INeedInfo
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        string Guid { get; }

        /// <summary>
        /// Время в секундах
        /// </summary>
        float Time { get; }

        /// <summary>
        /// Ключ меняется в зависимости от состояния котроллера
        /// </summary>
        string HeaderKey { get; }

        /// <summary>
        /// Пополняется ресурс
        /// </summary>
        bool IsRestock { get; }

        /// <summary>
        /// Нуждается в пополнении, идет простой
        /// </summary>
        bool IsNeed { get; }

        /// <summary>
        /// Не нуждается в пополнении
        /// </summary>
        bool IsIdle { get; }
    }
}