using AsciiPinyin.Web.Server.Controllers;
using AsciiPinyin.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AsciiPinyin.Web.Server.Commons;

public interface IEntityControllerCommons
{
    // 'Get' is some kind of a reserved word, it works, but VS shows a warning => using 'TheGet' instead.
    ActionResult<IEnumerable<T2>> TheGet<T1, T2>(
        T1 entityController,
        ILogger<T1> logger,
        string contextCollectionName
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity;

    ActionResult Post<T1, T2>(
        T1 entityController,
        T2 entity,
        string tableName,
        ILogger<T1> logger
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity;

    ActionResult PostDelete<T1, T2>(
        T1 entityController,
        T2 entity,
        string tableName,
        ILogger<T1> logger
    ) where T1 : ControllerBase, IEntityController where T2 : IEntity;
}
