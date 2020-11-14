﻿using Aga.Controls.Tree;
using fNbt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NbtStudio
{
    public class NbtFileNode : ModelNode
    {
        public readonly NbtFile File;
        public NbtFileNode(NbtTreeModel tree, INode parent, NbtFile file) : base(tree, parent)
        {
            File = file;
            File.RootTag.Changed += RootTag_Changed;
            File.RootTag.ActionPerformed += RootTag_ActionPerformed;
        }

        private void RootTag_ActionPerformed(object sender, UndoableAction e)
        {
            NoticeAction(e);
        }

        private void RootTag_Changed(object sender, NbtTag e)
        {
            RefreshChildren();
        }

        static NbtFileNode()
        {
            NodeRegistry.Register<NbtFile>((tree, parent, file) => new NbtFileNode(tree, parent, file));
        }

        protected override IEnumerable<object> GetChildren()
        {
            return File.RootTag;
        }

        public override string Description => File.Path == null ? "unsaved file" : System.IO.Path.GetFileName(File.Path);

        public override bool CanCopy => true;
        public override DataObject Copy()
        {
            var data1 = NbtNodeOperations.Copy(File.RootTag);
            var data2 = FileNodeOperations.Copy(File.Path);
            return Util.Merge(data1, data2);
        }
        public override bool CanCut => true;
        public override DataObject Cut()
        {
            var data1 = NbtNodeOperations.Copy(File.RootTag);
            var data2 = FileNodeOperations.Cut(File.Path);
            return Util.Merge(data1, data2);
        }
        public override bool CanDelete => true;
        public override void Delete()
        {
            FileNodeOperations.DeleteFile(File.Path);
        }
        public override bool CanEdit => File.Path != null;
        public override bool CanPaste => NbtNodeOperations.CanPaste(File.RootTag);
        public override bool CanRename => File.Path != null;
        public override bool CanSort => NbtNodeOperations.CanSort(File.RootTag);
        public override void Sort() => NbtNodeOperations.Sort(File.RootTag);
        public override IEnumerable<INode> Paste(IDataObject data)
        {
            var tags = NbtNodeOperations.Paste(File.RootTag, data);
            return FindChildren<NbtTagNode>(tags, x => x.Tag);
        }
        public override bool CanReceiveDrop(IEnumerable<INode> nodes) => nodes.All(x => x is NbtTagNode) && NbtNodeOperations.CanReceiveDrop(File.RootTag, nodes.Filter(x => x.GetNbtTag()));
        public override void ReceiveDrop(IEnumerable<INode> nodes, int index)
        {
            var tags = nodes.Filter(x => x.GetNbtTag());
            NbtNodeOperations.ReceiveDrop(File.RootTag, tags, index);
        }
    }
}
