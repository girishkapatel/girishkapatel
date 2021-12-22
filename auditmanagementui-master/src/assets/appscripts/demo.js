// import * as PptxGenJS from "C:/Users/hp/Downloads/PptxGenJS-3.4.0/dist/pptxgen.bundle";
// import * as PptxGenJS from "./pptxgen.bundle";





function exportToPPTJS() {
    
   var pptx = new PptxGenJS();
   pptx.layout = "LAYOUT_WIDE";
  var tabOpts = {
    x: 0,
    y: 1.0,
    w: "100%",
    h: "100%",
    fill: "F7F7F7",
    font_size: 14,
    color: "363636",
  };
  pptx.tableToSlides("divExportToPPT", null);
  pptx.writeFile(inTabId + "_" + getTimestamp());
}
