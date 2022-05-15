﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using Backend.Models;
using System.Text.Json.Nodes;
using System.Net.Http.Json;
using Aspose.Cells;
using Aspose.Cells.Utility;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.Data;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadData : ControllerBase
    {
        private readonly HttpClient http = new HttpClient();
        public static Hiperparametri hp = new Hiperparametri();
        public static string? Name { get; set; } //Ime ucitanog Csv fajla
        public static string? Username { get; set; } //Ulogovan korisnik
        public static string? DirName { get; set; } //Ime foldera 

        public static string url = "http://127.0.0.1:3000";

        public static string hiperJ;
        public static string modelSave;

        static String BytesToString(long byteCount) //proveriti sta ne radi kod ove funkcije
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; 
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }


        [HttpPost("selectedCsv")] //Otvaranje foldera gde se nalazi izabrani csv
        public async Task<ActionResult<String>> PostSelectedCsv(String name)
        {
            string fileName = name + ".csv";
            string CurrentPath = Directory.GetCurrentDirectory();
            //string SelectedPath = CurrentPath + @"\Users\" + Username + "\\" + name;
            string SelectedPath = Path.Combine(CurrentPath, "Users", Username, name);
            if (!System.IO.Directory.Exists(SelectedPath))
            {
                return BadRequest("Ne postoji dati fajl.");
            }
            else if (Username == null)
            {
                return BadRequest("Niste ulogovani.");
            }

            string[] files = Directory.GetFiles(SelectedPath).Select(Path.GetFileName).ToArray();

            //string SelectedPaths = CurrentPath + @"\Users\" + Username + "\\" + name + "\\" + fileName;
            string SelectedPaths = Path.Combine(CurrentPath, "Users", Username, name, fileName);
            /*var reader = new StreamReader(SelectedPaths);
            var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
            List<JsonDocument> cList = csv.GetRecords<JsonDocument>().ToList();*/

            var csvTable = new DataTable();
            using (var csvReader = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(System.IO.File.OpenRead(SelectedPaths)), true))
            {
                csvTable.Load(csvReader);
            }
            //csvTable.Rows.RemoveAt(csvTable.Rows.Count - 1);
            string result = string.Empty;
            result = JsonConvert.SerializeObject(csvTable);

            var resultjson = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(result); //json

            var data = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
            //var url = "http://127.0.0.1:3000/csv";
            var csvurl = url + "/csv";
            var response = await http.PostAsync(csvurl, data);

           
            long size = SelectedPaths.Length;
            var size1 = BytesToString(size);
            //Console.WriteLine("Size of file: " + size1);
            

            string fileName1 = SelectedPaths;
            FileInfo fi = new FileInfo(fileName1);
            DateTime creationTime = fi.CreationTime;
            //Console.WriteLine("Creation time: {0}", creationTime);
            Name = name;
            return Ok(resultjson);
        }

        [HttpPost("publicCsv")] //Otvaranje foldera gde se nalazi izabrani csv
        public async Task<ActionResult<String>> PostPublicCsv(String name)
        {
            string fileName = name + ".csv";
            string CurrentPath = Directory.GetCurrentDirectory();
            //string SelectedPath = CurrentPath + @"\Users\" + Username + "\\" + name;
            string SelectedPath = Path.Combine(CurrentPath, "Users", "publicDatasets", name);
            if (!System.IO.Directory.Exists(SelectedPath))
            {
                return BadRequest("Ne postoji dati fajl.");
            }
            
            string[] files = Directory.GetFiles(SelectedPath).Select(Path.GetFileName).ToArray();

            //string SelectedPaths = CurrentPath + @"\Users\" + Username + "\\" + name + "\\" + fileName;
            string SelectedPaths = Path.Combine(CurrentPath, "Users", "publicDatasets", name, fileName);
            /*var reader = new StreamReader(SelectedPaths);
            var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
            List<JsonDocument> cList = csv.GetRecords<JsonDocument>().ToList();*/

            var csvTable = new DataTable();
            using (var csvReader = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(System.IO.File.OpenRead(SelectedPaths)), true))
            {
                csvTable.Load(csvReader);
            }
            //csvTable.Rows.RemoveAt(csvTable.Rows.Count - 1);
            string result = string.Empty;
            result = JsonConvert.SerializeObject(csvTable);

            var resultjson = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(result); //json

            var data = new StringContent(result, System.Text.Encoding.UTF8, "application/json");
            //var url = "http://127.0.0.1:3000/csv";
            var csvurl = url + "/csv";
            var response = await http.PostAsync(csvurl, data);

            Name = name;
            return Ok(resultjson);
        }

        [HttpPost("savedModels")] //Vracanje imena sacuvanih Modela.
        public async Task<ActionResult<String>> PostSavedModels(String name)
        {
            DirName = name;
            RegistracijaUseraController.DirName = name;
            string CsvName = name;
            string CurrentPath = Directory.GetCurrentDirectory();
            //string SelectedPath = CurrentPath + @"\Users\" + Username + "\\" + CsvName;
            string SelectedPath = Path.Combine(CurrentPath, "Users", Username, CsvName);
            if (Username == null)
            {
                return BadRequest("Niste ulogovani.");
            }
            string[] subdirs = Directory.GetDirectories(SelectedPath).Select(Path.GetFileName).ToArray();

            return Ok(subdirs);
        }

        //predikcija
        [HttpPost("selectedModel")] //Vracanje imena izabranog modela. Tacnije putanje to je bitno zbog predikcije da znaju na ML-u koji model je korisnik izabrao
        public async Task<ActionResult<JsonDocument>> PostSelectedModel(String name)
        {
            if (Username == null)
            {
                return BadRequest("Niste ulogovani.");
            }
            string CurrentPath = Directory.GetCurrentDirectory();
            //string SelectedPath = CurrentPath + @"\Users\" + Username + "\\" + DirName + "\\" + name;
            string SelectedPath = Path.Combine(CurrentPath, "Users", Username, DirName, name);

            var modelName = name;
            var data = new StringContent(modelName, System.Text.Encoding.UTF8, "application/text");
            //var url = "http://127.0.0.1:3000/savedModel";
            var modelurl = url + "/savedModel";
            var response = await http.PostAsync(modelurl, data);
            return Ok(SelectedPath);

        }

        //za poredjenje dva modela
        [HttpPost("modelForCompare")] //Vracanje vrednosti izabranog modela kako bi mogle da se prikazu na grafiku i uporede.
        public async Task<ActionResult<JsonDocument>> PostModelForCompare(String dirname, String modelname)
        {
            string CurrentPath = Directory.GetCurrentDirectory();
            string fileName = modelname + ".csv";
            string SelectedPath = Path.Combine(CurrentPath, "Users", Username, dirname, modelname, fileName);

            if (!System.IO.File.Exists(SelectedPath))
            {
                return BadRequest("Ne postoji dati model. " + SelectedPath);
            }
            else if (Username == null)
            {
                return BadRequest("Niste ulogovani.");
            }
           
            var modelTable = new DataTable();
            using (var csvReader = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(System.IO.File.OpenRead(SelectedPath)), true))
            {
                modelTable.Load(csvReader);
            }
            string result = string.Empty;
            result = JsonConvert.SerializeObject(modelTable);

            var resultjson = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(result); 

            return Ok(resultjson);
        }
        
        [HttpPost("publicModels")] //Vracanje vrednosti izabranog javnog modela kako bi mogle da se prikazu na grafiku i uporede.
        public async Task<ActionResult<JsonDocument>> PostPublicModels(String userName, String modelname) //username je FromCsv a modelname je name
        {
            string CurrentPath = Directory.GetCurrentDirectory();
            string publicFolder = modelname + "(" + userName + ")";
            string fileName = modelname +"("+ userName +")"+ ".csv";
            string SelectedPath = Path.Combine(CurrentPath, "Users", "publicProblems", publicFolder, fileName);

            if (!System.IO.File.Exists(SelectedPath))
            {
                return BadRequest("Ne postoji dati model. " + SelectedPath);
            }

            var modelTable = new DataTable();
            using (var csvReader = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(System.IO.File.OpenRead(SelectedPath)), true))
            {
                modelTable.Load(csvReader);
            }
            string result = string.Empty;
            result = JsonConvert.SerializeObject(modelTable);

            var resultjson = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(result);

            return Ok(resultjson);
        }
        
        [HttpPost("save")] //pravljenje foldera gde ce se cuvati model cuva se model samo kad korisnik klikne na dugme sacuvaj model kao i cuvanje povratne vrednosti modela
        public async Task<ActionResult>Post(String modelNames, Boolean publicModel) //Ime modela kako korisnik zeli da ga cuva i da li zeli da bude javan model
        {
            if (Username != null)
            {
                string CurrentPath = Directory.GetCurrentDirectory();
                var upgradedName = Name;
                string path = Path.Combine(CurrentPath, "Users", Username, upgradedName);
                string modeldirname = Path.Combine(CurrentPath, "Users", Username, upgradedName, modelNames);
                //string modeldirname = upgradedName + modelNames;
                if (System.IO.Directory.Exists(modeldirname))
                {
                    return Ok("Vec postoji model sa tim imenom.");
                }
                else
                {
                    System.IO.Directory.CreateDirectory(modeldirname);
                    if (publicModel)
                    {
                        string publicName1 = modelNames + "(" + Username + ")";
                        string publicPath = Path.Combine(CurrentPath, "Users", "publicProblems", publicName1);
                        System.IO.Directory.CreateDirectory(publicPath);
                    }
                    Console.WriteLine("Directory for new Model created successfully!");
                }
                string saljemPath = CurrentPath + "\\Users\\" + Username + "\\" + upgradedName + "\\" + modelNames;
                var pathjson = System.Text.Json.JsonSerializer.Serialize(saljemPath);
                var pathdata = new StringContent(saljemPath, System.Text.Encoding.UTF8, "application/json");
                var pathurl = url + "/savemodel";
                var pathresponse = await http.PostAsync(pathurl, pathdata);


                //string hpName = modelNames + "HP.csv"; //treba da bude deletemeHP.csv pa da se obrise kada se skloni evaluate isto kao za model
                string hpName = "deletemeHP.csv";
                string path1 = Path.Combine(CurrentPath, "Users", Username, upgradedName, modelNames);
                string pathToCreateHP = System.IO.Path.Combine(path1, hpName);


                var workbookhp = new Workbook();
                var worksheethp = workbookhp.Worksheets[0];
                var layoutOptionshp = new JsonLayoutOptions();
                layoutOptionshp.ArrayAsTable = true;
                JsonUtility.ImportData(hiperJ, worksheethp.Cells, 0, 0, layoutOptionshp);


                var workbook = new Workbook();
                var worksheet = workbook.Worksheets[0];
                var layoutOptions = new JsonLayoutOptions();
                layoutOptions.ArrayAsTable = true;
                JsonUtility.ImportData(modelSave, worksheet.Cells, 0, 0, layoutOptions); 

                string modelName = "deleteme.csv";
                string pathToCreate = System.IO.Path.Combine(path, modelName);
                              
                workbook.Save(pathToCreate, SaveFormat.CSV); //cuvanje modela
                workbookhp.Save(pathToCreateHP, SaveFormat.CSV); //cuvanje hiperparametara

                List<String> lines = new List<string>();
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(pathToCreate);

                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);
                    //Console.WriteLine(line);
                }

                lines.RemoveAll(l => l.Contains("Evaluation Only."));

                List<String> lines1 = new List<string>();
                string line1;
                System.IO.StreamReader file1 = new System.IO.StreamReader(pathToCreateHP);

                while ((line1 = file1.ReadLine()) != null)
                {
                    lines1.Add(line1);
                }

                lines1.RemoveAll(l1 => l1.Contains("Evaluation Only."));

                string model = modelNames + ".csv";
                string publicName = modelNames + "(" + Username + ")";
                string pblmod = publicName + ".csv";
                string pathToCreate12 = System.IO.Path.Combine(modeldirname, model);
                string publicPathModel = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, pblmod);
                using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(pathToCreate12))
                {
                    outfile.Write(String.Join(System.Environment.NewLine, lines.ToArray()));
                }

                string hp1 = modelNames + "HP.csv";
                string pathToCreatehp = Path.Combine(CurrentPath, "Users", Username, upgradedName, modelNames, hp1);

                using (System.IO.StreamWriter outfile1 = new System.IO.StreamWriter(pathToCreatehp))
                {
                    outfile1.Write(String.Join(System.Environment.NewLine, lines1.ToArray()));
                }

                /*
                //modeldirname sacuvati i ona 3 niza kao txt fajl unutar ovog foldera
                string ColumnNamespath = Path.Combine(modeldirname, "ColumnNames.txt");
                List<string> ColumnLinesTxt = hp.ColumNames;   //hiper staviti u globalnu
                System.IO.File.WriteAllLines(ColumnNamespath, ColumnLinesTxt);

                string Encodingspath = Path.Combine(modeldirname, "Encodings.txt");
                List<string> EncodingsLinesTxt = hp.Encodings;
                System.IO.File.WriteAllLines(Encodingspath, EncodingsLinesTxt);

                string CatNumpath = Path.Combine(modeldirname, "CatNum.txt");
                List<string> CatNumLinesTxt = hp.CatNum;
                System.IO.File.WriteAllLines(CatNumpath, CatNumLinesTxt);
                */
                if (publicModel)
                {
                    using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(publicPathModel))
                    {
                        outfile.Write(String.Join(System.Environment.NewLine, lines.ToArray()));
                    }


                    string pblhp = publicName + "HP.csv";
                    string publicPathHp = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, pblhp);
                    using (System.IO.StreamWriter outfile1 = new System.IO.StreamWriter(publicPathHp))
                    {
                        outfile1.Write(String.Join(System.Environment.NewLine, lines1.ToArray()));
                    }
                    file1.Close();
                    //ColumNames Encodings CatNum
                    /*
                    string ColumnNames = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, "ColumnNames.txt");
                    List<string> ColumnlinesTxt = hp.ColumNames;
                    System.IO.File.WriteAllLines(ColumnNames, ColumnlinesTxt);

                    string Encodings = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, "Encodings.txt");
                    List<string> EncodingslinesTxt = hp.Encodings;
                    System.IO.File.WriteAllLines(Encodings, EncodingslinesTxt);

                    string CatNum = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, "CatNum.txt");
                    List<string> CatNumlinesTxt = hp.CatNum;
                    System.IO.File.WriteAllLines(CatNum, CatNumlinesTxt);*/
                }

                //string path1 = Directory.GetCurrentDirectory() + @"\Users\" + Username + "\\" + upgradedName;
                string path2 = System.IO.Path.Combine(CurrentPath, "Users", Username, upgradedName);
                string names = "deleteme.csv";
                string pathToDelete = System.IO.Path.Combine(path2, names);
                file.Close();
                if (System.IO.File.Exists(pathToCreate))
                {
                    System.IO.File.Delete(pathToCreate);
                }
                file1.Close(); 
                if (System.IO.File.Exists(pathToCreateHP))
                {
                    System.IO.File.Delete(pathToCreateHP);
                }

                return Ok(modeldirname);
            }
            else
                return Ok("Korisnik nije ulogovan.");
        }

        [HttpPost("hp")] //Slanje HP na pajton
        public async Task<ActionResult<Hiperparametri>> Post([FromBody] Hiperparametri hiper, String modelNames, Boolean publicModel) //pored hiperparametara da se posalje i ime modela kako korisnik zeli da ga cuva cuva se model pri svakom treniranju
        {
            int indexDir = 1;
            var upgradedName = "realestate";
            if(Name != null)
                upgradedName = Name.Substring(0, Name.Length - 4);
            //string path = Directory.GetCurrentDirectory() + @"\Users\" + Username + "\\" + upgradedName + "\\";
            string CurrentPath = Directory.GetCurrentDirectory();
            

            if (Username != null)
            {
                string path = Path.Combine(CurrentPath, "Users", Username, upgradedName);
                string modeldirname = Path.Combine(CurrentPath, "Users", Username, upgradedName, modelNames); //kada se prosledjuje ime zajedno sa hiperparametrima i uvek cuva
                //string modeldirname = upgradedName + modelNames;
                if (System.IO.Directory.Exists(modeldirname))
                {
                    return Ok("Vec postoji model sa tim imenom.");
                }
                else
                {
                    System.IO.Directory.CreateDirectory(modeldirname);
                    if(publicModel)
                    {
                        string publicName = modelNames + "(" + Username + ")";
                        string publicPath = Path.Combine(CurrentPath, "Users", "publicProblems", publicName);
                        System.IO.Directory.CreateDirectory(publicPath);
                    }   
                    Console.WriteLine("Directory for new Model created successfully!");
                }
                /*string modelDirName = upgradedName + "Model" + indexDir;
                string pathToCreateDir = System.IO.Path.Combine(path, modelDirName);
                while (System.IO.Directory.Exists(pathToCreateDir))
                {
                    indexDir++;
                    modelDirName = upgradedName + "Model" + indexDir;
                    pathToCreateDir = System.IO.Path.Combine(path, modelDirName);
                }
                System.IO.Directory.CreateDirectory(pathToCreateDir);
                Console.WriteLine("Directory for new Model created successfully!");*/

                var pathjson = System.Text.Json.JsonSerializer.Serialize(modeldirname);
                var pathdata = new StringContent(modeldirname, System.Text.Encoding.UTF8, "application/json");
                //var  = "http://127.0.0.1:3000/pathModel";
                var pathurl = url + "/pathModel";
                var pathresponse = await http.PostAsync(pathurl, pathdata);
            }
            
            hiper.Username = Username;
            var hiperjson = System.Text.Json.JsonSerializer.Serialize(hiper);
            var data = new StringContent(hiperjson, System.Text.Encoding.UTF8, "application/json");
            //var url = "http://127.0.0.1:3000/hp";
            var hpurl = url + "/hp";
            var response = await http.PostAsync(hpurl, data);            

            //                                                                                  hiperparametri                                                                                
            //---------------------------------------------------------------------------------------------------
            //                                                                                  model
            var modelurl = url + "/model";
            HttpResponseMessage httpResponse = await http.GetAsync(modelurl);
            //var model = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(await httpResponse.Content.ReadAsStringAsync()); 
            //var dataModel = await httpResponse.Content.ReadAsStringAsync(); 
            var dataModel = ""; //mora ovako dok se ne popravi primanje hiperparametara na ML delu
            if(Username != null)
            {
                int index = 1;
                //string hpName = modelNames + "HP" + index + ".csv";
                string hpName = modelNames + "HP.csv";
                string path = Path.Combine(CurrentPath, "Users", Username, upgradedName, modelNames);
                string pathToCreateHP = System.IO.Path.Combine(path, hpName);

                var workbookhp = new Workbook();
                var worksheethp = workbookhp.Worksheets[0];
                var layoutOptionshp = new JsonLayoutOptions();
                layoutOptionshp.ArrayAsTable = true;
                JsonUtility.ImportData(hiperjson, worksheethp.Cells, 0, 0, layoutOptionshp);


                var workbook = new Workbook();
                var worksheet = workbook.Worksheets[0];
                var layoutOptions = new JsonLayoutOptions();
                layoutOptions.ArrayAsTable = true;
                JsonUtility.ImportData(dataModel, worksheet.Cells, 0, 0, layoutOptions);

                //string modelName = modelNames + "Model" + index + ".csv";
                string modelName = "deleteme.csv";
                string pathToCreate = System.IO.Path.Combine(path, modelName);
                
                while (System.IO.File.Exists(pathToCreateHP))
                {
                    index++;
                    modelName = modelNames + "Model" + index + ".csv";
                    hpName = modelNames + "HP" + index + ".csv";
                    pathToCreate = System.IO.Path.Combine(path, modelName);
                    pathToCreateHP = System.IO.Path.Combine(path, hpName);
                }
               
                workbook.Save(pathToCreate, SaveFormat.CSV); //cuvanje modela
                workbookhp.Save(pathToCreateHP, SaveFormat.CSV); //cuvanje hiperparametara

                List<String> lines = new List<string>();
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader(pathToCreate);

                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);
                    //Console.WriteLine(line);
                }

                lines.RemoveAll(l => l.Contains("Evaluation Only."));
                
                string model = modelNames + ".csv";
                string publicName = modelNames + "(" + Username + ")";
                string pblmod = publicName + ".csv";
                string pathToCreate12 = System.IO.Path.Combine(path, model);
                string publicPathModel = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, pblmod);
                using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(pathToCreate12))
                {
                    outfile.Write(String.Join(System.Environment.NewLine, lines.ToArray()));
                }

                //modeldirname sacuvati i ona 3 niza kao txt fajl unutar ovog foldera
                string modeldirname = Path.Combine(CurrentPath, "Users", Username, upgradedName, modelNames);

                string ColumnNamespath = Path.Combine(modeldirname, "ColumnNames.txt");
                List<string> ColumnLinesTxt = hiper.ColumNames;
                System.IO.File.WriteAllLines(ColumnNamespath, ColumnLinesTxt);

                string Encodingspath = Path.Combine(modeldirname, "Encodings.txt");
                List<string> EncodingsLinesTxt = hiper.Encodings;
                System.IO.File.WriteAllLines(Encodingspath, EncodingsLinesTxt);

                string CatNumpath = Path.Combine(modeldirname, "CatNum.txt");
                List<string> CatNumLinesTxt = hiper.CatNum;
                System.IO.File.WriteAllLines(CatNumpath, CatNumLinesTxt);

                if (publicModel)
                {
                    using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(publicPathModel))
                    {
                        outfile.Write(String.Join(System.Environment.NewLine, lines.ToArray()));
                    }

                    List<String> lines1 = new List<string>();
                    string line1;
                    System.IO.StreamReader file1 = new System.IO.StreamReader(pathToCreateHP);

                    while ((line1 = file1.ReadLine()) != null)
                    {
                        lines1.Add(line1);
                    }

                    lines1.RemoveAll(l1 => l1.Contains("Evaluation Only."));

                    string pblhp = publicName + "HP.csv";
                    string publicPathHp = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, pblhp);
                    using (System.IO.StreamWriter outfile1 = new System.IO.StreamWriter(publicPathHp))
                    {
                        outfile1.Write(String.Join(System.Environment.NewLine, lines1.ToArray()));
                    }
                    file1.Close();
                    //ColumNames Encodings CatNum

                    string ColumnNames = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, "ColumnNames.txt");
                    List<string> ColumnlinesTxt = hiper.ColumNames;
                    System.IO.File.WriteAllLines(ColumnNames, ColumnlinesTxt);

                    string Encodings = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, "Encodings.txt");
                    List<string> EncodingslinesTxt = hiper.Encodings;
                    System.IO.File.WriteAllLines(Encodings, EncodingslinesTxt);

                    string CatNum = Path.Combine(CurrentPath, "Users", "publicProblems", publicName, "CatNum.txt");
                    List<string> CatNumlinesTxt = hiper.CatNum;
                    System.IO.File.WriteAllLines(CatNum, CatNumlinesTxt);
                }

                //string path1 = Directory.GetCurrentDirectory() + @"\Users\" + Username + "\\" + upgradedName;
                string path1 = System.IO.Path.Combine(CurrentPath, "Users", Username, upgradedName);
                string names = "deleteme.csv";
                string pathToDelete = System.IO.Path.Combine(path1, names);
                file.Close();
                if (System.IO.File.Exists(pathToCreate))
                {
                    System.IO.File.Delete(pathToCreate);
                }
                
            }
            else
                Console.WriteLine("Niste ulogovani.");
            return Ok(hiperjson);//model se vraca
        }


        [HttpPost("hpNeprijavljen")] //Slanje HP na pajton za neprijavljenog korisnika
        public async Task<ActionResult<Hiperparametri>> PostHp([FromBody] Hiperparametri hiper) 
        {
            if (Username == null)
                hiper.Username = "unknown";
            else
                hiper.Username = Username;
            hp = hiper;
            var hiperjson = System.Text.Json.JsonSerializer.Serialize(hiper);
            hiperJ = hiperjson;
            var data = new StringContent(hiperjson, System.Text.Encoding.UTF8, "application/json");
            //var url = "http://127.0.0.1:3000/hp";
            var hpurl = url + "/hp";
            var response = await http.PostAsync(hpurl, data);

            //                                                                                  hiperparametri                                                                                
            //---------------------------------------------------------------------------------------------------
            //                                                                                  model
            var modelurl = url + "/model";
            HttpResponseMessage httpResponse = await http.GetAsync(modelurl);
            var model = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(await httpResponse.Content.ReadAsStringAsync()); 
            var dataModel = await httpResponse.Content.ReadAsStringAsync(); 
            //var dataModel = ""; //mora ovako dok se ne popravi primanje hiperparametara na ML delu
            modelSave = dataModel;
            return Ok(model);//model se vraca ali dok se ne popravi na ML-u to mora ovako
        }

        [HttpPost("csv")] //Slanje CSV na pajton
        //[Obsolete]
        public async Task<ActionResult<DataLoad>> PostCsv([FromBody] DataLoad cs, Boolean publicData)
        {
            string name = cs.Name;
            string csve = cs.CsvData;
            Name = cs.Name;
            PythonController.Name = cs.Name;
            var data = new StringContent(csve, System.Text.Encoding.UTF8, "application/json");
            //var url = "http://127.0.0.1:3000/csv";
            var urlcsv = url + "/csv";
            var response = await http.PostAsync(urlcsv, data);

            var statsurl = url + "/stats";
            HttpResponseMessage httpResponse = await http.GetAsync(statsurl);
            var stat = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(await httpResponse.Content.ReadAsStringAsync());

            string currentPath = Directory.GetCurrentDirectory();
            //var upgradedName = name.Substring(0, name.Length - 4);
            //string path = currentPath + @"\Users\" + Username + "\\" + upgradedName;

            string pathToCreate = "";
            if (Username != null)
            {
                string path = System.IO.Path.Combine(currentPath, "Users", Username, name);
                string publicPath = System.IO.Path.Combine(currentPath, "Users", "publicDatasets", name);
                string publicName = name + ".csv";
                string publicCreate = System.IO.Path.Combine(publicPath, publicName);
                if (Directory.Exists(path))
                    Console.WriteLine("File is already in system.");
                else
                {
                    System.IO.Directory.CreateDirectory(path);
                    Console.WriteLine("Directory for '{0}' created successfully!", name);
                
                    var workbook = new Workbook();
                    var worksheet = workbook.Worksheets[0];
                    var layoutOptions = new JsonLayoutOptions();
                    layoutOptions.ArrayAsTable = true;
                    JsonUtility.ImportData(csve, worksheet.Cells, 0, 0, layoutOptions);

                    //string path = Directory.GetCurrentDirectory() + @"\Users\"+ Username;
                    string names = name + "1" + ".csv";
                    pathToCreate = System.IO.Path.Combine(path, names); 

                    workbook.Save(pathToCreate, SaveFormat.CSV);
            
                    List<String> lines = new List<string>();
                    string line;
                    System.IO.StreamReader file = new System.IO.StreamReader(pathToCreate);

                    while ((line = file.ReadLine()) != null)
                    {
                        lines.Add(line);
                        //Console.WriteLine(line);
                    }

                    lines.RemoveAll(l => l.Contains("Evaluation Only."));

                    string pathToCreate12 = System.IO.Path.Combine(path, name + ".csv");
                    using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(pathToCreate12))
                    {
                        outfile.Write(String.Join(System.Environment.NewLine, lines.ToArray()));
                    }

                    if(publicData)
                    {
                        if (Directory.Exists(publicPath))
                            Console.WriteLine("There is already public dataset with that name.");
                        else
                        {
                            System.IO.Directory.CreateDirectory(publicPath);
                            Console.WriteLine("Directory for '{0}' created successfully!", name);
                            //cuvanje dataseta na putanji publicPath i kreirati folder po imenu csv-a?
                            using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(publicCreate))
                            {
                                outfile.Write(String.Join(System.Environment.NewLine, lines.ToArray()));
                            }
                        }
                    }
                    file.Close();
                    System.IO.File.Delete(pathToCreate);
                    /*string CurrentPath = Directory.GetCurrentDirectory();
                    string SelectedPath = System.IO.Path.Combine(CurrentPath, "Users", Username);
                    string[] subdirs = Directory.GetDirectories(SelectedPath).Select(Path.GetFileName).ToArray();
                    for(int i = 0; i < subdirs.Length; i++)
                        Console.WriteLine(subdirs[i]);*/
                }
            }
            else
                Console.WriteLine("Niste ulogovani.");

            //System.IO.File.Delete(pathToCreate);
            return Ok(stat);
        }

        [HttpPost("predictionCsv")] //Slanje csv fajla za predikciju na pajton i primanje predikcije i prosledjivanje na front preko responsa.
        //[Obsolete]
        public async Task<ActionResult<DataLoad>> PostPredictedCsv([FromBody] DataLoad cs)
        {
            string name = cs.Name;
            string csve = cs.CsvData;
            Name = cs.Name;
            PythonController.Name = cs.Name;
            var data = new StringContent(csve, System.Text.Encoding.UTF8, "application/json");
            //var url = "http://127.0.0.1:3000/predictionCsv"; //slanje csv-a za prediktovanje na pajton
            var urlpred = url + "/predictionCsv";
            var response = await http.PostAsync(urlpred, data);

            var predurl = url + "/prediction";
            HttpResponseMessage httpResponse = await http.GetAsync(predurl); //rezultati predikcije
            var predikcija = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(await httpResponse.Content.ReadAsStringAsync());

            return Ok(predikcija);
        }

        //za prediktovanje csv-a preko modela
        [HttpPost("predictionModel")] //Slanje Putanje do foldera gde je sacuvan izabrani model na /pathModel bi mogao
                                      //Slanje originalnog CSV-a sa kojim je kreiran model na /csv mozda
                                      //Slanje hiperparametara sa kojima je kreiran izabrani model na /hp mozda
        public async Task<ActionResult<JsonDocument>> PostPredictedModel(String dirname, String modelname)
        {
            string CurrentPath = Directory.GetCurrentDirectory();
            string fileName = modelname + ".csv"; //rezultati modela koji su sacuvani u csv fajlu
            string SelectedPath = Path.Combine(CurrentPath, "Users", Username, dirname, modelname, fileName); //putanja do modela

            string csvName = dirname + ".csv"; //csv iz kojeg je kreiran model
            string csvPath = Path.Combine(CurrentPath, "Users", Username, dirname, csvName); //putanja do csv-a

            string hpName = modelname + "HP.csv"; //hiperparametri korisceni za kreiranje izabranog modela
            string hpPath = Path.Combine(CurrentPath, "Users", Username, dirname, modelname, hpName); //putanja do hiperparametara

            if (!System.IO.File.Exists(csvPath)) //dal postoji taj csv
            {
                return BadRequest("Ne postoji dati model. " + SelectedPath);
            }
            else if (Username == null)
            {
                return BadRequest("Niste ulogovani.");
            }

            string modelPath = Path.Combine(CurrentPath, "Users", Username, dirname, modelname); //putanja do foldera gde je model
            var datamodel = new StringContent(modelPath, System.Text.Encoding.UTF8, "application/json");
            var modelurl = url + "/pathModel";
            var responsemodel = await http.PostAsync(modelurl, datamodel);

            //CSV IZ KOJEG JE KREIRAN MODEL
            var csvTable = new DataTable();
            using (var csvReader = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(System.IO.File.OpenRead(csvPath)), true))
            {
                csvTable.Load(csvReader);
            }
            string resultCSV = string.Empty;
            resultCSV = JsonConvert.SerializeObject(csvTable);

            var resultjsoncsv = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(resultCSV); //rezultati csv-a to treba poslati na /csv
            var datacsv = new StringContent(resultCSV, System.Text.Encoding.UTF8, "application/json");
            var csvurl = url + "/csv";
            var responsecsv = await http.PostAsync(csvurl, datacsv);

            //HIPERPARAMETRI SA KOJIMA JE KREIAN MODEL
            var hpTable = new DataTable();
            using (var csvReader = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(System.IO.File.OpenRead(hpPath)), true))
            {
                hpTable.Load(csvReader);
            }
            string resultHP = string.Empty;
            resultHP = JsonConvert.SerializeObject(hpTable);

            var resultjsonhp = System.Text.Json.JsonSerializer.Deserialize<JsonDocument>(resultHP); //rezultati HP to treba poslati na /hp
            var datahp = new StringContent(resultHP, System.Text.Encoding.UTF8, "application/json");
            var hpurl = url + "/hp";
            var responsehp = await http.PostAsync(hpurl, datahp);


            return Ok(resultjsonhp);
        }
        [HttpPost("stats")] //Slanje Stats na pajton
        public async Task<ActionResult<Statistika>> PostStat(Statistika stat)
        {
            var statjson = System.Text.Json.JsonSerializer.Serialize(stat);
            var data = new StringContent(statjson, System.Text.Encoding.UTF8, "application/json");
            //var url = "http://127.0.0.1:3000/stats";
            var urlst = url + "/stats";
            var response = await http.PostAsync(urlst, data);
            return Ok(statjson);
        }

    }
}
