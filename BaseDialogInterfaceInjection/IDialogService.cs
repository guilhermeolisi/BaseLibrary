using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IDialogService
{
    Task<string[]?> OpenFile(Tuple<List<string>, string>[] extensions, bool allowMultiple, string? initialFolder, string? initialFile = null);
    Task<string?> SaveFile(Tuple<List<string>, string>[] extensions, bool allowMultiple, string? initialFolder = null, string? initialFile = null);
    Task<string?> SelectFolder(string? initialFolder = null);
    Task<bool?> ConfirmDialog(string message, string[] buttons);
    Task<string?> GetOneEntryDialog(string message, string entry, string[] buttons);
}
