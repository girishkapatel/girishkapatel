export interface tableData {
  [key: string]: any;
}

export interface tableColumn {
  title: string;
  className?: string;
  data: string;
  dataId?: string;
  render?(data?: any, type?: any, row?: any, meta?: any): any;
  orderable?:boolean;
}
