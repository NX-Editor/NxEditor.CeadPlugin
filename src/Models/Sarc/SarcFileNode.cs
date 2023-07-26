﻿using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using NxEditor.EpdPlugin.ViewModels;
using NxEditor.PluginBase.Models;
using System.Collections.ObjectModel;

namespace NxEditor.EpdPlugin.Models.Sarc;

public partial class SarcFileNode : ObservableObject
{
    private TextBox? _renameClient = null;

    [ObservableProperty]
    private SarcFileNode? _parent;

    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SarcFileNode> _children = new();

    [ObservableProperty]
    private bool _isRenaming;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isSelected;

    public IFileHandle? Handle { get; private set; }
    public byte[] Data => Handle?.Data ?? throw new InvalidDataException("The node does not have any data!");
    public bool IsFile => Handle != null;
    public string? PrevName { get; set; }

    public SarcFileNode(string header, SarcFileNode? parent = null)
    {
        _header = header;
        _parent = parent;
    }

    public void Sort()
    {
        Children = new(Children.OrderBy(x => x.Header));
        foreach (var child in Children) {
            child.Sort();
        }
    }

    public void BeginRename()
    {
        _renameClient?.SelectAll();
        _renameClient?.Focus();
        PrevName = Header;

        IsRenaming = true;
    }

    public void EndRename(SarcEditorViewModel owner)
    {
        owner.History.StageChange(SarcChange.Rename, new List<(SarcFileNode, object?)>() {
            (this, PrevName)
        });
        owner.RenameMapNode(this);

        IsRenaming = false;
    }

    public async Task ExportAsync(string path, bool recursive = true, bool isSingleFile = false, SarcFileNode? relativeTo = null)
        => await Task.Run(() => Export(path, recursive, isSingleFile, relativeTo));
    public void Export(string path, bool recursive = true, bool isSingleFile = false, SarcFileNode? relativeTo = null)
    {
        if (IsFile && relativeTo != null) {
            Directory.CreateDirectory(path = Path.Combine(Path.GetDirectoryName(path)!, GetPath(relativeTo)));
            using FileStream fs = File.Create(Path.Combine(path, Header));
            fs.Write(Data);
        }
        else if (IsFile) {
            Directory.CreateDirectory(isSingleFile ? Path.GetDirectoryName(path)! : path);
            using FileStream fs = File.Create(isSingleFile ? path : Path.Combine(path, Header));
            fs.Write(Data);
        }
        else {
            foreach (var file in GetFileNodes(recursive)) {
                file.Export(Path.Combine(path, file.GetPath(relativeTo)));
            }
        }
    }

    public string GetFilePath(SarcFileNode? relativeTo = null) => Path.Combine(GetPathParts(relativeTo).Append(Header).ToArray());
    public string GetPath(SarcFileNode? relativeTo = null) => Path.Combine(GetPathParts(relativeTo).ToArray());
    public Stack<string> GetPathParts(SarcFileNode? relativeTo = null)
    {
        Stack<string> parts = new();

        if (!IsFile && Header != "__root__") {
            parts.Push(Header);
        }

        SarcFileNode? parent = Parent;
        while (parent != null && parent != relativeTo && parent.Header != "__root__") {
            parts.Push(parent.Header);
            parent = parent.Parent;
        }

        return parts;
    }

    public IEnumerable<SarcFileNode> GetFileNodes(bool recursive = true)
    {
        IEnumerable<SarcFileNode> result;
        if (!IsFile) {
            result = Children.Where(x => x.IsFile);
            foreach (var child in Children.Where(x => !x.IsFile)) {
                result = result.Concat(child.GetFileNodes(recursive));
            }
        }
        else {
            result = new SarcFileNode[1] { this };
        }

        return result;
    }

    public void SetData(byte[] data)
    {
        Handle = new FileHandle(data) {
            Name = Header
        };
    }

    internal void SetRenameClient(TextBox? renameClient)
    {
        _renameClient = renameClient;
    }

    public SarcFileNode Clone()
    {
        return (SarcFileNode)MemberwiseClone();
    }
}
