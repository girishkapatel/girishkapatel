﻿var ComponentsBootstrapMultiselect=function(){return{init:function(){$(".mt-multiselect").each(function(){var t,a=$(this).attr("class"),i=!!$(this).data("clickable-groups")&&$(this).data("clickable-groups"),l=!!$(this).data("collapse-groups")&&$(this).data("collapse-groups"),o=!!$(this).data("drop-right")&&$(this).data("drop-right"),e=(!!$(this).data("drop-up")&&$(this).data("drop-up"),!!$(this).data("select-all")&&$(this).data("select-all")),s=$(this).data("width")?$(this).data("width"):"",n=$(this).data("height")?$(this).data("height"):"",d=!!$(this).data("filter")&&$(this).data("filter"),h=function(t,a,i){alert("Changed option "+$(t).val()+".")},r=function(t){alert("Dropdown shown.")},c=function(t){alert("Dropdown Hidden.")},p=1==$(this).data("action-onchange")?h:"",u=1==$(this).data("action-dropdownshow")?r:"",b=1==$(this).data("action-dropdownhide")?c:"";t=$(this).attr("multiple")?'<li class="mt-checkbox-list"><a href="javascript:void(0);"><label class="mt-checkbox"> <span></span></label></a></li>':'<li><a href="javascript:void(0);"><label></label></a></li>',$(this).multiselect({enableClickableOptGroups:i,enableCollapsibleOptGroups:l,disableIfEmpty:!0,enableFiltering:d,includeSelectAllOption:e,dropRight:o,buttonWidth:s,maxHeight:n,onChange:p,onDropdownShow:u,onDropdownHide:b,buttonClass:a})})}}}();jQuery(document).ready(function(){ComponentsBootstrapMultiselect.init()});