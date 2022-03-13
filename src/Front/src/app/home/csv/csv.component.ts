import { Component } from "@angular/core";

@Component({
    selector: 'app-csv',
    templateUrl: 'csv.component.html'
})
export class CsvComponent {

    headingLines: any = [];
    rowLines: any = [];

    fileUpload(files: any) {
        let fileList = (<HTMLInputElement>files.target).files;
        if (fileList && fileList.length > 0) {
            let file : File = fileList[0];
            console.log(file.name);

            let reader: FileReader =  new FileReader();
            reader.readAsText(file);
            reader.onload = (e) => {
                let csv: any = reader.result;
                let allTextLines = [];
                allTextLines = csv.split('\n');
                
                let headers = allTextLines[0].split(/;|,/);
                let data = headers;
                let headersArray = [];

                for (let i = 0; i < headers.length; i++) {
                    headersArray.push(data[i]);
                }
                this.headingLines.push(headersArray);

                let rowsArray = [];

                let length = allTextLines.length - 1;
                
                let rows = [];
                for (let i = 1; i < length; i++) {
                    rows.push(allTextLines[i].split(/;|,/));
                }
                length = rows.length;
                for (let j = 0; j < length; j++) {
                    rowsArray.push(rows[j]);
                }
                this.rowLines.push(rowsArray);
                console.log(this.rowLines);
            }
        }
    }
}