using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NTree<T>
{
    private LinkedList<NTree<T>> children;
    private T data;

    public NTree(T data)
    {
        this.data = data;
        this.children = new LinkedList<NTree<T>>();
    }

    public void AddDeepChild(params T[] traverse)
    {
        LinkedList<NTree<T>> children = this.children;
        foreach (T local in traverse)
        {
            bool flag = false;
            foreach (NTree<T> tree in children)
            {
                if (EqualityComparer<T>.Default.Equals(tree.data, local))
                {
                    children = tree.children;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                NTree<T> tree2 = new NTree<T>(local);
                children.AddLast(tree2);
                children = tree2.children;
            }
        }
    }

    public void SetData(T data)
    {
        this.data = data;
    }

    private void traverse(NTree<T> node, TreeVisitor<T> visitor, TreePreTraverse previsitor, TreePostTraverse postvisitor, int ignoredepth)
    {
        if (((visitor == null) || (ignoredepth >= 0)) || visitor(node.data))
        {
            foreach (NTree<T> tree in node.children)
            {
                if ((previsitor != null) && (ignoredepth < 0))
                {
                    previsitor();
                }
                this.traverse(tree, visitor, previsitor, postvisitor, ignoredepth - 1);
                if ((postvisitor != null) && (ignoredepth < 0))
                {
                    postvisitor();
                }
            }
        }
    }

    public void Traverse(TreeVisitor<T> visitor, TreePreTraverse previsitor, TreePostTraverse postvisitor, int ignoredepth = -1)
    {
        this.traverse((NTree<T>) this, visitor, previsitor, postvisitor, ignoredepth);
    }
}

