import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { ActionComponent } from "./action.component";
import { ActionAddComponent } from "./add/action-add.component";
import { ActionEditComponent } from "./edit/action-edit.component";
import { ActionViewComponent } from "./view/action-view.component";

const routes: Routes = [
  {
    path: "",
    component: ActionComponent,
    data: {
      title: "Action",
    },
    children: [
      {
        path: "",
        component: ActionViewComponent,
      },
      {
        path: "add",
        component: ActionAddComponent,
      },
      {
        path: "edit",
        component: ActionEditComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ActionRoutingModule {}
