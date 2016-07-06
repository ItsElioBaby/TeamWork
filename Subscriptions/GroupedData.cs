using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Q1;
using twlib;

[Serializable]
public class DataObject
{
    private string name;
    private object value;

    public string ObjectName { get { return name; } }
    public object ObjectValue { get { return value; } }

    public DataObject(string n, object v)
    {
        name = n;
        value = v;
    }
}

public class GroupDataWriter
{
    List<DataObject> objects = new List<DataObject>();

    public void WriteObject(DataObject o)
    {
        objects.Add(o);
    }

    public void WriteAway(string file)
    {
        int len = objects.Count;
        MemoryStream memsr = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memsr);

        writer.Write(len);

        RRR.BasicQ1BinaryFormation bfmt = new RRR.BasicQ1BinaryFormation();

        foreach (DataObject o in objects)
        {
            byte[] bts = Serialization.BinarySerialize(o);
            writer.Write(bts.Length);
            writer.Write(bts);
        }

        writer.Flush();
        byte[] file_bytes = bfmt.Patch(memsr.ToArray());
        File.WriteAllBytes(file, file_bytes);
    }
}

public class GroupDataReader
{
    List<DataObject> objects = new List<DataObject>();

    public GroupDataReader(string file)
    {
        byte[] f = File.ReadAllBytes(file);
        if(f.Length > 0)
        {
            RRR.BasicQ1BinaryFormation bfmt = new RRR.BasicQ1BinaryFormation();

            byte[] orig = bfmt.DePatch(f);
            MemoryStream memsr = new MemoryStream(orig);

            BinaryReader reader = new BinaryReader(memsr);
            int c = reader.ReadInt32();
            for (int i = 0; i < c; i++)
            {
                int b_len = reader.ReadInt32();
                byte[] bts = reader.ReadBytes(b_len);
                DataObject o = (DataObject)Serialization.BinaryDeserialize(bts);
                objects.Add(o);
            }
            reader.Close();
        }
    }

    public DataObject[] GetObjects()
    {
        if (objects.Count < 1)
        {
            List<DataObject> nDT = new List<DataObject>();
            nDT.Add(new DataObject("NULL", null));
            return nDT.ToArray();
        }
        return objects.ToArray();
    }

    public DataObject[] GetObjects(string name)
    {
        return (from o in objects where o.ObjectName == name orderby o.ObjectName select o).ToArray();
    }

    public DataObject[] GetObjects(object value)
    {
        return (from o in objects where o.ObjectValue == value orderby o.ObjectValue select o).ToArray();
    }
}

public class GroupDataManager
{
    GroupDataWriter writer;
    GroupDataReader reader;

    string fname;

    public GroupDataManager(string file)
    {
        if (!File.Exists(file))
        {
            File.Create(file).Close();
        }
        reader = new GroupDataReader(file);
        writer = new GroupDataWriter();

        
        fname = file;

        foreach (DataObject o in reader.GetObjects())
        {
            objects.Add(o);
        }
    }

    public bool Contains(DataObject obj)
    {
        return objects.Contains(obj);
    }

    public GroupDataManager()
    {
        writer = new GroupDataWriter();
    }

    List<DataObject> objects = new List<DataObject>();

    public void AddObject(DataObject o)
    {
        objects.Add(o);
    }

    public void Remove(string name)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].ObjectName == name)
            {
                objects.RemoveAt(i);
            }
        }
    }

    public void Remove(object value)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i].ObjectValue == value)
            {
                objects.RemoveAt(i);
            }
        }
    }

    public bool Contains(string name)
    {
        foreach (DataObject o in objects)
        {
            if (o.ObjectName == name)
                return true;
        }
        return false;
    }

    public GroupDataReader Reader { get { return reader; } }

    public void Remove(DataObject o)
    {
        if (objects.Contains(o))
            objects.Remove(o);
    }

    public void Save(bool verbose)
    {
        int done = 0;
        foreach (DataObject o in objects)
        {
            if(verbose)
            {
                Console.Clear();
                Console.WriteLine("Dumping data to \"" + fname + "\"..." + (done / objects.Count) * 100 + "%");
                done++;
            }
            writer.WriteObject(o);
        }
        writer.WriteAway(fname);
    }

    public void Save(string file)
    {
        foreach (DataObject o in objects)
            writer.WriteObject(o);
        writer.WriteAway(file);
    }
}