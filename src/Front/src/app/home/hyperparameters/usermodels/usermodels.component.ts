import { Component, OnInit , Output, EventEmitter} from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ModalDismissReasons,NgbModal} from '@ng-bootstrap/ng-bootstrap';
import * as myUrls from 'src/app/urls';
import {RefreshService} from './usermodels.service'

interface zapamceniModeli {
  name: String,
  fromCsv: String,
  date: Date
}

@Component({
  selector: 'app-usermodels',
  templateUrl: './usermodels.component.html',
  styleUrls: ['./usermodels.component.css']
})

export class UsermodelsComponent implements OnInit {

  @Output() sendResults = new EventEmitter<any>();
  //ovim saljemo nazad ka hyperparamteres komponenti model

  constructor(private http: HttpClient, private refreshModels: RefreshService,
    private modalService: NgbModal){
    
  }
  public url = myUrls.url;
  zapamceniModeli: any;
  selectedModels:any;
  searchModels: any;
  loadedCsvPrediction:boolean;

  ngOnInit(): void {
    this.getModels();
    this.refreshModels.messageSubject.subscribe({
      next: x => {
      //  console.log("REFRESH COMPONENTE");
          this.getModels();
      }
    });

    this.loadedCsvPrediction = false;
  }

  getModels()
  {
    var loggedUsername = sessionStorage.getItem('username');
    this.http.get<any>(this.url + '/api/Python/savedModels?Username='+loggedUsername).subscribe(result => {  //uzima nazive svih datasetova od ulogovanog korisnika
           // console.log(result);
            this.zapamceniModeli=result;
          //  console.log(this.zapamceniModeli);
        });

        this.selectedModels = '';
  }

  selectModels(event:any){
    this.selectedModels=event.target.value;
  //  console.log('ovo je kliknuto za naziv '+this.selectedModels);
    //alert(this.selectedModels);
    this.loadThisModel('ime_models');
  }

  loadThisModel(naziv:any){
    var loggedUsername = sessionStorage.getItem('username');
    return this.http.post<any>(this.url + '/api/LoadData/selectedModel?name='+naziv+'&Username='+loggedUsername, {
        name: naziv
      }).subscribe(selectedModelUser=>{
        
       // console.log(selectedModelUser);
        //alert("SALJEMO RES");

        this.sendResults.emit(selectedModelUser);
        // sada ovo saljemo nazad da se podese hiperparametri


      });
    }
    
  selektovanDirName:string;
  selektovanModelName:string;
  ulogovanKorisnik :any;

  closeResult: string | undefined;
  addNewData(newData: any, event:any, item:any){
    this.selektovanDirName='';
    this.selektovanModelName='';
    this.ulogovanKorisnik ='';

    this.selektovanModelName=event.target.id;
    this.selektovanDirName=item;
    this.ulogovanKorisnik = sessionStorage.getItem('username');
  
   // console.log('na ovaj csv: '+ this.selektovanDirName);
    // console.log('na ovaj model kliknuto: '+ this.selektovanModelName);

     this.modalService.open(newData, {ariaLabelledBy: 'modal-basic-title'}).result.then((result) => {
       this.closeResult = `Closed with: ${result}`;
     }, (reason) => {
        this.closeResult = `Dismissed ${this.getDismissReason(reason)}`;
        this.loadedCsvPrediction = false;
      });
   }


   private getDismissReason(reason: any): string {
     if (reason === ModalDismissReasons.ESC) {
       return 'by pressing ESC';
     } else if (reason === ModalDismissReasons.BACKDROP_CLICK) {
       return 'by clicking on a backdrop';
     } else {
       return `with: ${reason}`;
     }
   }

   dataObject:any = [];
   headingLines: any = [];
   rowLines: any = [];
   ucitano:boolean=false;
   unetDataset:string='';

   predikcija:any = [];

   fileUpload(files: any) {
    this.loadedCsvPrediction = true;
    this.unetDataset='';
    this.ucitano=false;
    this.prikaziPredikciju=false;

     this.dataObject = [];
     this.headingLines = [];
     this.rowLines = [];
     this.predikcija = [];

       let fileList = (<HTMLInputElement>files.target).files;
       if (fileList && fileList.length > 0) {
           let file : File = fileList[0];
         //  console.log('file name:'+file.name);
           this.unetDataset=file.name;
           let reader: FileReader =  new FileReader();
           reader.readAsText(file);
           reader.onload = (e) => {
               let csv: any = reader.result;
               let allTextLines = [];
               allTextLines = csv.split('\n');
                
               let headers = allTextLines[0].split(/;|,/).map((x:string) => x.trim());
               let data = headers;
               let headersArray = [];


               for (let i = 0; i < headers.length; i++) {
                headersArray.push(data[i]);
            }
            this.headingLines.push(headersArray);

            let rowsArray = [];

            let length = allTextLines.length - 1;
            
            let rows:any = [];
            for (let i = 1; i < length; i++) {
              rows.push(allTextLines[i].split(/;|,/).map((x:string) => x.trim()));
              const obj:any = {};
              headersArray.forEach((header:any, j:any) => {
                  obj[header] = rows[i - 1][j];
              })
              this.dataObject.push(obj);

            }
            length = rows.length;
            for (let j = 0; j < length; j++) {
                rowsArray.push(rows[j]);
            }
            
            this.rowLines = rowsArray.slice(0, this.itemsPerPage);
            this.allData = rowsArray;

            
            this.http.post<any>(this.url+'/api/LoadData/predictionModel?dirname='+this.selektovanDirName+'&modelname='+this.selektovanModelName+'&Username='+this.ulogovanKorisnik, {
              csvData: JSON.stringify(this.dataObject),
              Name: file.name
            }).subscribe( result=>{
              this.predikcija=result;
              
             //   console.log('ovo je predikcija'+this.predikcija);
            });

           }
       }
   }

   addNewDataset()
   {
       this.ucitano=true;
       document.getElementById("closeModal")?.click();
   }
   

   allData: any = [];
    itemsPerPage: number = 6;
    itemPosition: number = 0;
    currentPage: number = 1;

    changePage() {
      this.rowLines = this.allData.slice(this.itemsPerPage * (this.currentPage - 1),this.itemsPerPage * (this.currentPage - 1) + this.itemsPerPage)
  }

  deleteModelName:any;
  deleteModelFromCsv:any;
  deleteThisModel(event:any, item:any){
   
    this.deleteModelName=event.target.id;
    this.deleteModelFromCsv=item;
    var loggedUsername = sessionStorage.getItem('username');
    this.http.delete<any>(this.url +'/api/RegistracijaUsera/model?fromCsv='+this.deleteModelFromCsv+'&name='+this.deleteModelName+'&Username='+loggedUsername).subscribe(result => { 
   //   console.log(result);

      this.ngOnInit();
     }, (err)=>{
       console.log("Greska prilikom brisanja modela "+err);
       this.ngOnInit();
     });
    
    this.deleteModelName = "";
    this.deleteModelFromCsv = "";
    event.stopPropagation();
    this.ngOnInit();
    this.ucitano=false;
      this.prikaziPredikciju=false;
  }

  prikaziPredikciju:boolean=false;

  showPrediction(){
    this.prikaziPredikciju=true;
  }

  obrisiSearchInputField()
  {
      this.searchModels = "";
  }
}
