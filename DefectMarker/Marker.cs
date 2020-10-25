using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DefectMarker
{
    class Marker
    {
        public List<MarkerFileData> MarkerFiles = new List<MarkerFileData>();
        public MarkerFileData CurrentMarkerFile = new MarkerFileData();

        public void LoadMarkerFiles(string FileName)
        {
            using (StreamReader sr = new StreamReader(FileName))
            {
                string json = sr.ReadToEnd();
                MarkerFiles = JsonConvert.DeserializeObject<List<MarkerFileData>>(json);
            }
        }

        public void SaveMarkerFiles(string FileName)
        {
            using (StreamWriter file = File.CreateText(FileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, MarkerFiles);
            }

            string json = JsonConvert.SerializeObject(MarkerFiles);

        }

        public void CreateMarkerFile(string FileName)
        {
            MarkerFiles.Add(new MarkerFileData()
            {
                _FileName = FileName,
                _MarkerList = new List<MarkerItem>()
            });
        }

        public bool FileNameExist(string FileName)
        {
            var query = this.MarkerFiles.Where(mf => mf._FileName == FileName);
            if (query.Count() > 0) { return true; } else { return false; }
        }

        public void SetCurrentMarkerFile(string FileName)
        {
            CurrentMarkerFile = new MarkerFileData();
            var query = this.MarkerFiles.Where(mf => mf._FileName == FileName);
            if (query.Count() == 1) { CurrentMarkerFile = query.First(); };
        }

        public void WriteMarkerData(string FileName, int Xs, int Ys, int Xe, int Ye)
        {
            if (CurrentMarkerFile._FileName == FileName)
            {
                CurrentMarkerFile._MarkerList.Add(new MarkerItem() { _Xs = Xs, _Ys = Ys, _Xe = Xe, _Ye = Ye });
            }
            else
            {
                throw new Exception("File Name Not Match");
            }
        }

        public void DeleteLastMarkerData(string FileName)
        {
            if (CurrentMarkerFile._FileName == FileName)
            {
                CurrentMarkerFile._MarkerList.RemoveAt(CurrentMarkerFile._MarkerList.Count() - 1);
            }
            else
            {
                throw new Exception("File Name Not Match");
            }
        }

        public List<MarkerItem> ReadMarkerList(string FileName)
        {
            CurrentMarkerFile = new MarkerFileData();
            var query = this.MarkerFiles.Where(mf => mf._FileName == FileName);
            if (query.Count() == 1)
            {
                CurrentMarkerFile = query.First();
            }
            else { throw new Exception("Marker Data Not Match"); }
            return CurrentMarkerFile._MarkerList;
        }
        public class MarkerFileData
        {
            public string _FileName;
            public List<MarkerItem> _MarkerList;
        }


        public class MarkerItem
        {
            public int _Xs, _Ys, _Xe, _Ye;
        }
    }
}
