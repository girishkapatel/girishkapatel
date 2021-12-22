import { DatePipe } from "@angular/common";
import { Injectable } from "@angular/core";
import { FormGroup, FormControl } from "@angular/forms";
import { NgbDate } from "@ng-bootstrap/ng-bootstrap";
import { utc } from "moment";
import { ToastrService } from "ngx-toastr";

@Injectable()
export class UtilsService {
  constructor(
    private datePipe: DatePipe,
    private notifyService: ToastrService
  ) { }

  monthNames = [
    "Jan",
    "Feb",
    "Mar",
    "Apr",
    "May",
    "Jun",
    "Jul",
    "Aug",
    "Sep",
    "Oct",
    "Nov",
    "Dec",
  ];

  isEmptyObj(obj: any) {
    for (var prop in obj) {
      if (obj.hasOwnProperty(prop)) {
        return false;
      }
    }
    return true;
  }

  getDropwonHtml(data: any, config: { [key: string]: string }) {
    let optHtml = ``;

    if (config.placeholder) {
      optHtml += `<option>${config.placeholder}</option>`;
    }

    if (data.length) {
      for (let optdata of data) {
        optHtml += `<option value="${optdata[config.value]}">${optdata[config.label]
          }</option>`;
      }
    }
    return optHtml;
  }

  formatJSONDate(dateStr) {
    let start = parseInt(
      dateStr.replace(/\/Date\((.*?)[+-]\d+\)\//i, "$1"),
      10
    );
    let newDate = new Date(dateStr);

    return (
      newDate.getDate() +
      "-" +
      this.monthNames[newDate.getMonth()] +
      "-" +
      newDate.getFullYear()
    );
  }

  getCurrentDate() {
    let date = new Date();

    return (
      date.getDate() +
      "-" +
      this.monthNames[date.getMonth()] +
      "-" +
      date.getFullYear()
    );
  }

  formatDate(dateStr) {
    if (dateStr.indexOf("T") > -1) {
      let dateSplit = dateStr
        .slice(0, dateStr.indexOf("T"))
        .replace(/-/g, "/")
        .split("/");

      return (
        dateSplit[2] +
        "-" +
        this.monthNames[parseInt(dateSplit[1]) - 1] +
        "-" +
        dateSplit[0]
      );

      // return dateStr.slice(0, dateStr.indexOf("T")).replace(/-/g, "/");
    } else {
      return dateStr;
    }
  }

  formatDateYYYYMMDD(dateStr) {
    if (dateStr.indexOf("T") > -1) {
      let dateSplit = dateStr
        .slice(0, dateStr.indexOf("T"))
        .replace(/-/g, "/")
        .split("/");
      return dateSplit[0] + "-" + dateSplit[1] + "-" + dateSplit[2];
    } else if (dateStr) {
      if (dateStr) return this.datePipe.transform(dateStr, "dd-MMM-yyyy");
    } else return dateStr;
  }

  formatDMYToYMD(dateStr) {
    let dateSplit = dateStr.split("/");

    return dateSplit[2] + "-" + dateSplit[1] + "-" + dateSplit[0];
  }

  formatNgbDateToYMD(ngbDateObj) {
    if (ngbDateObj) {
      var myDate = this.formatNgbDateToDate(ngbDateObj);

      return this.datePipe.transform(myDate.toLocaleString(), "yyyy-MM-dd");
    }
  }

  formatDbDateToDMY(dbDate) {
    if (dbDate) {
      let dateObj = this.formatToNgbDate(dbDate);

      let ngbDate = this.formatNgbDateToDate(dateObj);

      let returnDate = this.formatDateToStr(ngbDate);
      return returnDate;
    }
    return "";
  }

  formatNgbDateToDMY(dateObj) {
    if (dateObj) {
      let returnObj = this.formatDateToStr(this.formatNgbDateToDate(dateObj));

      return returnObj;
    }
    return "";
  }

  formatDateToStr(dateObj) {
    return this.datePipe.transform(dateObj.toLocaleString(), "dd/MM/yyyy");
  }

  formatNgbDateToDate(ngbDate) {
    return new Date(ngbDate.year, ngbDate.month - 1, ngbDate.day);
  }

  formatNgbDateToYMDpopup(ngbDateObj) {
    var myDate = this.formatNgbDateToDate(ngbDateObj);

    return this.datePipe.transform(myDate.toLocaleString(), "dd/MM/yyyy");
  }

  formatToNgbDate(dateStr) {
    if (dateStr) {
      if (dateStr.indexOf("T") > -1) {
        let dateSplit = dateStr
          .slice(0, dateStr.indexOf("T"))
          .replace(/-/g, "/")
          .split("/");

        return new NgbDate(
          Number(dateSplit[0]),
          Number(dateSplit[1]),
          Number(dateSplit[2])
        );
      } else {
        let dateSplit = dateStr.split("/");

        return new NgbDate(
          Number(dateSplit[2]),
          Number(dateSplit[1]),
          Number(dateSplit[0])
        );
      }
    }
  }

  getCurrentDateddmmyyyy() {
    let date = new Date();

    return (
      date.getDate() + "/" + (date.getMonth() + 1) + "/" + date.getFullYear()
    );
  }

  UTCtoLocalDate(dateStr) {
    var myDate = new Date(dateStr);
    return this.datePipe.transform(myDate.toLocaleDateString(), "yyyy-MM-dd");
  }

  checkIfApprover() {
    let isApprover = localStorage.getItem("isApprover");
    if (typeof isApprover === "string" && isApprover === "true") {
      return true;
    } else {
      return false;
    }
  }

  compareDate(dateStr, condition) {
    let isValid = false;

    try {
      let dateObj = new Date(dateStr);
      switch (condition) {
        case "gt":
          isValid = dateObj > new Date();
          break;
        case "gteq":
          isValid = dateObj >= new Date();
          break;
        case "lt":
          isValid = dateObj < new Date();
          break;
        case "lteq":
          isValid = dateObj <= new Date();
          break;
        case "eq":
          isValid = dateObj === new Date();
          break;
        default:
          isValid = false;
      }
    } catch {
      isValid = false;
    }
    return isValid;
  }

  getCurrentFinancialYear() {
    var startYear;
    var endYear;
    var docDate = new Date();
    if (docDate.getMonth() + 1 <= 3) {
      startYear = docDate.getFullYear() - 1;
      endYear = docDate.getFullYear();
    } else {
      startYear = docDate.getFullYear();
      endYear = docDate.getFullYear() + 1;
    }
    return { startDate: "01/04/" + startYear, endDate: "31/03/" + endYear };
  }
  getCurrentFinancialYearByDate(date1, date2) {
    var fiscalyear = "";
    var fDate = new Date(date1);
    var sDate = new Date(date2);
    if (fDate.getMonth() + 1 <= sDate.getMonth()) {
      fiscalyear =
        "FY " +
        (fDate.getFullYear() - 1).toString().substr(-2) +
        "-" +
        fDate.getFullYear().toString().substr(-2);
    } else {
      fiscalyear =
        "FY " +
        fDate.getFullYear().toString().substr(-2) +
        "-" +
        (fDate.getFullYear() + 1).toString().substr(-2);
    }
    return fiscalyear;
  }

  getScopeObj() {
    let scopeObj = [];
    let uiScope = localStorage.getItem("uis");

    if (uiScope) {
      scopeObj = JSON.parse(uiScope);
    }
    return scopeObj;
  }

  getSubmoduleAccess(moduleName) {
    let routes;
    let scopeObj = this.getScopeObj();

    if (scopeObj.length) {
      routes = scopeObj.filter((x) => {
        let rname = x.name.replace(/\s/g, "").toLowerCase();
        return rname === moduleName;
      });
    }

    return routes;
  }

  getAccessOnLevel1(moduleName, subModuleName) {
    var moduleObj = this.getSubmoduleAccess(moduleName)[0];

    let submodule = moduleObj.submodules.filter(
      (x) => x.name.replace(/\s/g, "").toLowerCase() === subModuleName
    )[0];

    let accessRights = {
      name: submodule.name,
      access: submodule.access,
      isAdd: submodule.isAdd,
      isEdit: submodule.isEdit,
      isDelete: submodule.isDelete,
      submodules: submodule.submodules,
    };

    return accessRights;
  }

  todecimalRound(data) {
    let num = (Math.round((data + Number.EPSILON) * 100) / 100).toFixed(2);
    return num;
  }

  globalDateFormat(date) {
    let pipe = new DatePipe("en-US");
    return pipe.transform(date, "dd-MMM-yy");
  }
  UTCtoLocalDateDDMMMYY(dateStr) {
    if (dateStr != null) {
      var myDate = new Date(dateStr);
      return this.datePipe.transform(myDate.toLocaleDateString(), "dd/MM/yyyy");
    }
  }
  UTCtoLocalDates(dateStr) {
    var myDate = new Date(dateStr);
    return this.datePipe.transform(myDate.toLocaleString(), "dd/MM/yyyy");
  }
  calculateDiff(dateSent) {
    if (dateSent) {
      let currentDate = new Date();
      // var data = dateSent.split('/'); 
      var newdate = dateSent.split("/").reverse().join("/");
      var oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds
      var firstDate = new Date(); // 29th of Feb at noon your timezone
      var secondDate = new Date(newdate); // 2st of March at noon
      var day = Math.floor((secondDate.getTime() - firstDate.getTime()) / (oneDay));
      return day;
      // console.log(firstDate, "to", secondDate, "\nDifference: " + diffDays + " day");

      // dateSent = new Date(data[2], data[1], data[0]);
      // dateSent = new Date(, ,); 
      // var cc = this.datePipe.transform(currentDate.toLocaleString(),"dd/MM/yyyy");
      // var cd=new Date(cc);
      //  var dd = this.datePipe.transform(dateSent.toLocaleString(),"dd/MM/yyyy");
      //  var dateObject=new Date(dd);
      //  return Math.floor(Date.UTC(dateSent.getFullYear(), dateSent.getMonth(), dateSent.getDate())-(Date.UTC(cd.getFullYear(), cd.getMonth(), cd.getDate())) / (1000 * 60 * 60 * 24));
    }
    return 0;
  }
  // getNumberofDay(dbDate) {
  //   if (dbDate) {
  //     let dt = new Date();
  //      let today= this.formatDbDateToDMY(dt);
  //     Date() newDate = new Date(dbDate);
  //     let differnceTime = today - newDate.getTime();
  //     let differenceDay = differnceTime / (1000 * 3600 * 24);
  //     return differenceDay;
  //   }
  //   return "";
  // }
  dtRatingColor = {
    partiallyreceived: "#FFC200",
    received: "#2c973e",
    pending: "#f04c3e",
  };
  convertLakhsandCarors(value, type) {
    var val = Math.round(value)
    switch (type) {
      case "LAC":
        value = (val / 100000).toFixed(0);
        break;
      case "CR":
        value = (val / 10000000).toFixed(0);
        break;
      default:
        value;
    }
    return value;
  }

  showValidationMsg(formGroup: FormGroup) {
    let isErrorOccurred = false;

    for (const key in formGroup.controls) {
      if (formGroup.controls.hasOwnProperty(key)) {
        const control: FormControl = <FormControl>formGroup.controls[key];

        if (Object.keys(control).includes("controls")) {
          const formGroupChild: FormGroup = <FormGroup>formGroup.controls[key];
          this.showValidationMsg(formGroupChild);
        }

        if (!isErrorOccurred && control.errors) {
          this.notifyService.error("Please complete all the required fields (*) to continue!");

          isErrorOccurred = true;
        }

        control.markAsTouched();
      }
    }
  }
}
