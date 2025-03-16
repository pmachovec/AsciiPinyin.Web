using AsciiPinyin.Web.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AsciiPinyin.Web.Server.Delegates;

public delegate bool AreConflictDbItegrityErrors<T>(T entity, DbSet<Chachar> knownChachars, DbSet<Alternative> knownAlternatives, out IEnumerable<string> errors);
