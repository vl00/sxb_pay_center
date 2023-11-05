/// 删除目录(sln)下的bin和obj文件夹

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

del_binobj_dirs(Directory.GetCurrentDirectory());
//del_binobj_dirs(Path.Combine(Directory.GetCurrentDirectory(), "Fix"));
Console.WriteLine("ok");
//Console.ReadLine();

static void del_binobj_dirs(string dirPath)
{
    var dirs = yeild_binobj_dirs(new DirectoryInfo(dirPath))
        .Where(dir => 
        {
            if (Regex.IsMatch(dir, ".\\\\([Bb]in|[Oo]bj)\\\\."))
            {
                Console.WriteLine(dir);
                return false;
            }
            return true;
        })
        .ToArray();

    foreach (var dir in dirs)
    {
        Directory.Delete(dir, true);
    }
}

static IEnumerable<string> yeild_binobj_dirs(DirectoryInfo dir)
{
    foreach (var d in dir.EnumerateDirectories())
    {
        foreach (var _d in yeild_binobj_dirs(d))
            yield return _d;
    }
    if (dir.EnumerateFiles().Any(f => f.Extension == ".csproj" || f.Extension == ".xproj"))
    {
        var r = dir.GetDirectories("bin").FirstOrDefault();
        if (r == null) r = dir.GetDirectories("Bin").FirstOrDefault();
        if (r != null) yield return r.FullName;

        r = dir.GetDirectories("obj").FirstOrDefault();
        if (r == null) r = dir.GetDirectories("Obj").FirstOrDefault();
        if (r != null) yield return r.FullName;
    }
}