$(document).ready(function () {
    window.addEventListener("pageshow", function (e) {
        var historyTraversal = e.persisted || (typeof window.performance != "undefined" &&
            this.window.performance.navigation.type === 2);
        if (historyTraversal) {
            window.location.reload();
        }
    })
    setTimeout(hideLoader, 500);
    var focusedIndex = -1;
    $('#rightSide .has-search input').val("");
    let searchWidth = $('#rightSide .has-search input').css("width");
    let searchHeight = $('#rightSide .has-search input').css("height");
    $(".search-options-list").css("width", searchWidth);
    $(".search-options-list .option").css("height", searchHeight);
    

    function showLoader() {
        console.log("show");
        $(".PageLoader").addClass("active");
        $(".PageLoader").addClass("d-flex");
    }

    function hideLoader() {
        console.log("hide");
        $(".PageLoader").removeClass("active");
        $(".PageLoader").removeClass("d-flex");
    }



    function changeDynamicLink(appId) {

        $(".dynamicLink").each(function () {
            var staticLink = $(this).attr("data-Link");
            var newLink = staticLink + "?appId=" + appId;
            $(this).attr("href", newLink);
        })
        $(".form form").attr("action", $("span#formRoute").attr("href"));
    }

    function stringMatch(value, domain) {
        return domain && domain.trim().toLowerCase().indexOf(value.trim().toLowerCase()) >= 0;
    }

    $(document).on("click", ".Register .formStep .btnLink", function (e) {
        e.preventDefault();
        letTargetForm = $(this).attr("data-target");
        $(".formStep").removeClass("active");
        $("#" + letTargetForm).addClass("active");

        return false;
    });

    $(document).on("click", "a.staticLink", function (e) {
        
        e.preventDefault();
        e.stopPropagation();
        let linkClicked = $(this);
        console.log("link clicked")
        showLoader();
        setTimeout(function () {
            window.location = linkClicked.attr("href");
        }, 300)
        return false;
        
       
    });

    $(document).on("click", "button.staticBtn", function (e) {
        e.preventDefault();
        showLoader();
        let btnClicked = $(this);
        setTimeout(function () {
            let form = btnClicked.parents("form").first();
            form.submit();
        },300)
    
        return false;
    });

    $(document).on("click", "#leftSide .selectedInfo .selected", function () {
       
        if ($(window).width() < 767) {
            showLoader();
            $("#leftSide").addClass("inactive");
            $("#rightSide").addClass("active");
            setTimeout(hideLoader, 300);
        }
    });

    $(".accordion .smaller-card,.smaller-card").click(function () {
        showLoader();
        //ignore if already active
        if (!$(this).hasClass("Active")) {

            let link = $(this).attr("href");
            //if it has a link then bypass by redirecting
            if (link != "#") {
                $(".default.smaller-card").trigger("click");
                window.open(link);
            }
            else {
                let title = $(this).find(".smaller-card-title").html();
                let description = $(this).find(".smaller-card-details").html();
                let classname = $(this).attr("class");
                let appId = $(this).attr("data-Id")
                classname = classname.replace(" ", ".");
                $(".accordion .smaller-card").removeClass("Active");
                $("#leftSide .selectedInfo .left-sub-acronym").html(title);
                $("#leftSide .selectedInfo .left-sub-title").html(description)
               
                changeDynamicLink(appId);

                if ($(window).width() < 767) {
                    $("#leftSide").removeClass("inactive");
                    $("#rightSide").removeClass("active");
                }

                $("." + classname).addClass("Active");
            }


            //finally show all card and accordion
            $(".smaller-card").show();
            $(".accordion .card").show();
        }
        else {
            if ($(window).width() < 767) {
                $("#leftSide").removeClass("inactive");
                $("#rightSide").removeClass("active");
            }
        }

        setTimeout(hideLoader, 300);
        return false;
    });

    $("#rightSide .has-search input").keyup(function (e) {
        let inputVal = $(this).val();

        //  $(".search-options-list .option").removeClass("Match");
        //  $(".search-options-list .option").removeClass("active");
        //  $(".search-options-list .option").each(function(){$(this).attr("Id",undefined)});

        if (inputVal.length > 0 && e.keyCode != "13") {
            $(".accordion .card .smaller-card").hide();
            // console.log( $(".accordion .card .smaller-card").hide());
            //hide all accordion
            $(".accordion .card").hide();


            let count = 0;

            $(".search-options-list .option").each(function () {

                let currentOption = $(this);
                let title = currentOption.find(".option-title").text();
                let description = currentOption.find(".option-details").text();

                if (stringMatch(inputVal, title) || stringMatch(inputVal, description)) {
                    currentOption.show();
                    var optionId = currentOption.attr("data-Id");
                    let associatedCard = $("." + optionId);
                    associatedCard.show();
                    associatedCard.each(function () { $(this).parents(".accordion .card").first().show() })

                    currentOption.addClass("Match");
                    currentOption.attr("Id", count + "Match")
                    count++;
                }
                else {
                    currentOption.hide();
                    // currentOption.removeClass("Match")
                    // $(".search-options-list .option").each(function(){$(this).attr("Id",null)});
                }

            });

            if (count > 0) {
                // focusedIndex=-1;
                $(".search-options-list").show();
            }
            else {


                $(".search-options-list").hide();
            }

        }
        else {
            $(".smaller-card").show();
            $(".accordion .card").show();
            $(".search-options-list").hide();
        }

    });

    $("#rightSide .has-search input").keydown(function (e) {

        if (e.keyCode == '40' || e.keyCode == "38" || e.keyCode == "13") {
            //get every navigable option
            var navgOpt = $(".search-options-list .option.Match");
            var navgOptLength = navgOpt.length;
            if (navgOptLength <= 0) {
                return false;
            }
            if (e.keyCode == "40") {
                focusedIndex++;
                //remove every active class
                var active = $(".has-search .active").removeClass("active");

                if (focusedIndex >= navgOptLength) {
                    focusedIndex = -1;
                    // $("search").addClass("active");
                }
                else {
                    var focusedElem = $("#" + focusedIndex + "Match");
                    focusedElem.addClass("active");
                }
            }
            else if (e.keyCode == "38") {
                focusedIndex--;
                //remove every active class
                $(".has-search .active").removeClass("active");
                if (focusedIndex < 0) {
                    focusedIndex = navgOptLength - 1;
                    var focusedElem = $("#" + focusedIndex + "Match");
                    focusedElem.addClass("active");
                }
                else if (focusedIndex == -1) {
                    // $("search").addClass("active");
                }
                else {
                    var focusedElem = $("#" + focusedIndex + "Match");
                    focusedElem.addClass("active");
                }
            }
            else {
                // 
                // $(".search-options-list").hide();
                console.log("enter pressed")
                var focusedElem = $("#" + focusedIndex + "Match");
                if (focusedElem.get(0) != null) {
                    focusedElem.trigger("click");
                }



            }
        }
        else {
            $(".search-options-list .option").removeClass("active");
            focusedIndex = -1;
            $(".search-options-list .option").each(function () { $(this).attr("Id", null) });
        }
    })

    $("#rightSide .search-options-list .option").click(function () {
        let value = $(this).find(".option-title").text();
        $('#rightSide .has-search input').val(value);

        let optionId = $(this).attr("data-Id");
        $("." + optionId).first().trigger("click");
        $(".search-options-list").hide();

    })

    $(window).resize(function () {
        let searchWidth = $('#rightSide .has-search input').css("width");
        let searchHeight = $('#rightSide .has-search input').css("height");
        $(".search-options-list").css("width", searchWidth);
        $(".search-options-list .option").css("height", searchHeight);
    })

    $(document).click(function () {
        //$(".NotificationLauncher").trigger("click");
        $(".search-options-list").hide();
    });

    $(document).on("click", ".navLink", function (e) {
        
        e.preventDefault();
        e.stopPropagation();
        showLoader();
            console.log("here");
            $(this).attr("disable", true);
            var url = $(this).attr("href");

            var pageType = $(this).attr("data-Page");

            $.ajax({
                url: url,
                type: "GET",
                success: function (data, status) {
                    //console.log(data);
                    $("div.main").removeClass("Register");
                    $("div.main").removeClass("Login");
                    $("div.main").addClass(pageType);
                    $("#changeContents").empty();
                    $("#changeContents").append(data);
                    setTimeout(hideLoader, 300);
                },
                error: function (xhr, desc, err) {
                    $(".modal").modal("hide");
                    $(".modal-backdrop").remove();
                    $("#notificationModal .notificationIcon").removeClass("active")


                    let title = $("#notificationModal").find(".modal-title .title");
                    let content = $("#notificationModal").find(".modal-body");
                    $("#notificationModal .failure.notificationIcon").addClass("active")
                   
                    if (xhr.status == "404") {
                        title.html("Not Found");
                        content.html("Sorry, the page you requested for cannot be found or is temporarily unavailable");
                    }
                    else {
                        title.html("Server Error");
                        content.html("Oops!, an error occured while processing request, kindly reload page and try again, if error persist contact NUPRC support for help");
                    }
                    $("#notificationModal").modal("show");
                    setTimeout(hideLoader, 500);
                }
            });
        
       return false;
       

    })


    //------------------PostRequestHandler----------------------------------//

    $(document).on("click","form .SubmitBtn", function (e) {
        //  let buttonClicked = $(this);
        e.preventDefault();
        e.stopPropagation();

        showLoader();
            $(this).attr("disable", true);
        let formElement = $(this).parents("form").first();
        console.log(formElement.serializeArray())
            $.ajax({
                url: formElement.attr("action"),
                type: "POST",
                data: formElement.serialize(),
                success: function (data, status) {
                     //clear validation error and the messages
                    $(".modal").modal("hide");
                    $(".modal-backdrop").remove();
                    $("#notificationModal .notificationIcon").removeClass("active")
                    $("input,select").removeClass("input-validation-error");
                    $("span.field-validation-error").html("");
                    $("div.ErrorSummary").html("");

                    //console.log(data);
                    //return false;
                    if (data.Result == "Success") {
                        $("#leftSide form")[0].reset();
                        window.location = data.ReturnUrl;
                    }
 
                    else if (data.Result == "Notification") {

                        let title = $("#notificationModal").find(".modal-title .title");
                        let content = $("#notificationModal").find(".modal-body");

                        title.html(data.Title);
                        content.html(data.Message);

                        if (data.ResultType == "Success") {
                            $("#leftSide form")[0].reset();
                            $("#notificationModal .success.notificationIcon").addClass("active")
                        }
                        else if (data.ResultType == "Failure") {
                            $("#notificationModal .failure.notificationIcon").addClass("active")
                        }

                        $("#notificationModal").modal("show");
                        setTimeout(hideLoader, 300);

                    }
                    else if (data.Result == "PageDefined") {
                        if (data.Message == "MailNotification") {
                            $("#MailNotification form").attr("action",data.ReturnUrl);
                        }

                        if (data.ResultType == "Success") {
                            $("#leftSide form")[0].reset();
                        }

                        $("#" + data.Message).modal("show");
                        setTimeout(hideLoader, 300);
                    }
                    else {
                        $("#changeContents").empty();
                        $("#changeContents").append(data);
                        setTimeout(hideLoader, 300);
                    }

                },
                error: function (xhr, desc, err) {
                   // console.log(xhr.responseText);
                    if (xhr.status == "404") {
                        console.log("test");
                        return false;
                        window.location = "/account/NotFound";
                    }
                    else {
                        window.location = "/account/error";
                    }
                }
            });
        return false;
    })


    $(document).on("click", ".AffilateBtn", function () {
        
        $(".modal").modal("hide");
        $(".modal-backdrop").remove();
        showLoader();
            let bodyElement = $(this).parents("body .main.Register").first();
            let formElement = bodyElement.find("#leftSide .form form").first();
            let url = bodyElement.find("#affilateRoute").attr("href");

            console.log(bodyElement);
            console.log(formElement.serialize());
            console.log(url);

            $.ajax({
                url: bodyElement.find("#affilateRoute").attr("href"),
                type: "POST",
                data: formElement.serialize(),
                success: function (data, status) {
                    $("#notificationModal .notificationIcon").removeClass("active")
                    console.log(data.Result);
                    if (data.Result == "Notification") {
                        let title = $("#notificationModal").find(".modal-title .title");
                        let content = $("#notificationModal").find(".modal-body");

                        title.html(data.Title);
                        content.html(data.Message);

                        $("#leftSide form")[0].reset();
                        $("#notificationModal .notificationIcon.success").addClass("active")

                        $("#notificationModal").modal("show");
                        

                    }
                    else {

                        $("#changeContents").empty();
                        $("#changeContents").append(data);
                        //$("#errorModalCenter").modal('show');
                    }
                    setTimeout(hideLoader, 300);
                },
                error: function (xhr, desc, err) {
                    if (xhr.status == "404") {
                        window.location = "/account/NotFound";
                    }
                    else {
                        window.location = "/account/error";
                    }
                }
            });
        return false;
    })





    //$(document).on("click", ".form form .RegisterBtn", function (e) {
    //    //  let buttonClicked = $(this);
    //    e.preventDefault();
    //    e.stopPropagation();

    //    $(this).attr("disable", true);
    //    let formElement = $(this).parents("form").first();

    //    $.ajax({
    //        url: formElement.attr("action"),
    //        type: "POST",
    //        data: formElement.serialize(),
    //        success: function (data, status) {

    //            $("#notificationModal .notificationIcon").removeClass("active")
    //            //clear validation error and the messages
    //            $("input,select").removeClass("input-validation-error");
    //            $("span.field-validation-error").html("");
    //            $("div.ErrorSummary").html("");

    //            //console.log(data);
    //            //return false;
    //            if (data.Result == "Notification") {
    //                let title = $("#notificationModal").find(".modal-title .title");
    //                let content = $("#notificationModal").find(".modal-body");

    //                title.html(data.Title);
    //                content.html(data.Message);

    //                if (data.ResultType == "Success") {
    //                    $("#leftSide form")[0].reset();
    //                }

    //                $("#notificationModal").modal("show");

    //            }
    //            else if (data.Result == "PageDefined") {
    //                if (data.ResultType == "Success") {
    //                    $("#leftSide form")[0].reset();
    //                    $("#notificationModal .notificationIcon.success").addClass("active")
    //                }
    //                else if (data.ResultType == "Failure") {
    //                    $("#notificationModal .notificationIcon.failure").addClass("active")
    //                }

    //                $("#" + data.Message).modal("show");
    //            }
    //            else {
    //                $("#changeContents").empty();
    //                $("#changeContents").append(data);
    //            }

    //        },
    //        error: function (xhr, desc, err) {
    //            if (err = "404") {
    //                window.location = "/Account/NotFound";
    //            }
    //            window.location = "/Account/Error";
             
    //        }
    //    });
    //    return false;
    //})

    //$(document).on("click", ".MailBtn", function () {
       

    //    $(".modal").modal("hide");
    //    $(".modal-backdrop").remove();

    //    $.ajax({
    //        url: $("#MailNotification form").attr("action"),
    //        type: "POST",
    //        success: function (data, status) {
    //            $("#notificationModal .notificationIcon").removeClass("active")
    //            console.log(data.Result);
    //            if (data.Result == "Notification") {
    //                let title = $("#notificationModal").find(".modal-title .title");
    //                let content = $("#notificationModal").find(".modal-body");

    //                title.html(data.Title);
    //                content.html(data.Message);

    //                if (data.ResultType == "Success") {
    //                    $("#leftSide form")[0].reset();
    //                    $("#notificationModal .notificationIcon.success").addClass("active");
    //                }
    //                else if (data.ResultType == "Failure") {
    //                    $("#notificationModal .notificationIcon.failure").addClass("active");
    //                }
    //                $("#notificationModal").modal("show");

    //            }

    //        },
    //        error: function (xhr, desc, err) {
    //            displayAlert("Error! An error occured while processing the request, try again", "fail", $("#changeLayer"));
    //        }
    //    });
    //    return false;
    //})




    //$(document).on("click", ".form form .SubmitBtn", function (e) {
    //    //  let buttonClicked = $(this);
    //    e.preventDefault();
    //    e.stopPropagation();

    //    $(this).attr("disable", true);
    //    let formElement = $(this).parents("form").first();
    //  //  let url = formElement.attr("action");

    //    //response Expected: Success, Failure, Notification

    //    //when success is returned: show the loading page and log the user In the user: Parameter1 represents the username,Parameter 2 represent the password
    //    //if there is any failure: notify the customer and keep the Page Open
    //    //if thre is any Notification: change content to login and notify the customer
    //    //if there is an error: do not notify just redisplay page with error


    //    $.ajax({
    //        url: formElement.attr("action"),
    //        type: "POST",
    //        data: formElement.serialize(),
    //        success: function (data, status) {
    //            //clear validation error and the messages
    //            $("input").removeClass("input-validation-error");
    //            $("select").removeClass("input-validation-error");
    //            $("span.field-validation-error").html("");
    //            $("div.ErrorSummary").html("");
    //            //console.log(data);
    //            //return false;
    //            if (data.Result == "Success") {
    //                $("#leftSide form")[0].reset();
    //                //Process the apphere
    //                //show the loading Page indicating redirect

    //                //set the input values
    //                if (data.Parameter1 != null) {
    //                    $("#RedirectHandler form .email").val(data.Parameter1);
    //                    $("#RedirectHandler form .code").val(data.Parameter2);
    //                    $('#RedirectHandler form').attr("action", data.returnUrl);

    //                    //Post this action:
    //                    $("RedirectHandler form").submit();
    //                }
    //                else {
    //                    window.location = data.ReturnUrl;
    //                }
    //            }
    //            else if (data.Result == "Notification") {

    //                console.log("#notificationModal");
    //                let title = $("#notificationModal").find(".modal-title");
    //                let content = $("#notificationModal").find(".modal-body");

    //                title.html("Notification");
    //                content.html(data.Message);

    //                $("#leftSide form")[0].reset();

    //                $("#notificationModal").modal("show");
    //            }
    //            else if (data.Result == "PageDefined") {
    //                 $("#" + data.Message).modal("show");
    //            }
    //            else if (data.Result == "Failure") {
    //                $("errorModalCenter")
    //            }
    //            else {
    //                $("#changeContents").empty();
    //                $("#changeContents").append(data);
    //                //$("#errorModalCenter").modal('show');
    //            }

    //        },
    //        error: function (xhr, desc, err) {
    //            displayAlert("Error! An error occured while processing the request, try again", "fail", $("#changeLayer"));
    //        }
    //    });
    //    return false;
    //})


    //$(document).on("click", ".AffilateBtn", function () {
    //    $(".modal").modal("hide");
    //    $(".modal-backdrop").remove();
    //    let bodyElement = $(this).parents("body .main.Register").first();
    //    let formElement = bodyElement.find("#leftSide .form form").first();
    //    let url = bodyElement.find("#affilateRoute").attr("href");

    //    console.log(bodyElement);
    //    console.log(formElement.serialize());
    //    console.log(url);

    //    $.ajax({
    //        url: bodyElement.find("#affilateRoute").attr("href"),
    //        type: "POST",
    //        data: formElement.serialize(),
    //        success: function (data, status) {
    //            console.log(data.Result);
    //            if (data.Result == "Success") {
    //                $("#leftSide form")[0].reset();
    //                window.location = data.ReturnUrl;
    //            }
    //            else {

    //                $("#changeContents").empty();
    //                $("#changeContents").append(data);
    //                $("#errorModalCenter").modal('show');
    //            }

    //        },
    //        error: function (xhr, desc, err) {
    //            displayAlert("Error! An error occured while processing the request, try again", "fail", $("#changeLayer"));
    //        }
    //    });
    //    return false;
    //})



    //$(document).on("click", ".AffilateBtn", function () {
    //    let bodyElement = $(this).parents("body .main.Register").first();
    //    let formElement = bodyElement.find("#leftSide .form form").first();
    //    let url = bodyElement.find("#affilateRoute").attr("href");
    //    formElement.attr("action", url);
    //    let submitBtn = formElement.find("button.SubmitBtn");
    //    submitBtn.trigger("click");
    //});





































    //$(document).on("click", "button.SubmitBtn", function () {
    //    let buttonClicked = $(this);
    //    $(this).attr("disable", true);
    //    let formElement = $(this).parents("form").first();
    //    let url = formElement.attr("action");


    //    $.ajax({
    //        url: formElement.attr("action"),
    //        type: "POST",
    //        data: formElement.serialize(),
    //        success: function (data, status) {
    //            console.log(data.Result);
    //            if (data.Result == "Success") {
    //                //Process the apphere
    //            }
    //            else if (data.Result == "Notification") {
    //                console.log("inside notification");
    //                console.log($('#notification'));
    //                $("#notification").modal('show');
    //            }
    //            else if (data.Result == "Failure") {
                   
    //            }
    //            else {
    //                $("#changeContents").empty();
    //                $("#changeContents").append(data);
    //                $("#errorModalCenter").modal('show');
    //            }
               
    //        },
    //        error: function (xhr, desc, err) {
    //            displayAlert("Error! An error occured while processing the request, try again", "fail", $("#changeLayer"));
    //        }
    //    });
    //    return false;
    //})



});
















//$(document).ready(function () {
//    var focusedIndex=-1;
//    $('#rightSide .has-search input').val("");
//    let searchWidth=$('#rightSide .has-search input').css("width");
//    let searchHeight= $('#rightSide .has-search input').css("height");
//    $(".search-options-list").css("width",searchWidth);
//    $(".search-options-list .option").css("height",searchHeight);

//    function stringMatch(value,domain){
//        return domain && domain.trim().toLowerCase().indexOf(value.trim().toLowerCase())>=0;
//    }


//    $(".Register .formStep .btnLink").click(function(e){
//        e.preventDefault();
//       letTargetForm= $(this).attr("data-target");
//       $(".formStep").removeClass("active");
//       $("#"+letTargetForm).addClass("active");
//       return false;
//    })



//    $(".accordion .smaller-card").click(function () {
//        let appId = $(this).attr("data-Id");
//        let title=$(this).find(".smaller-card-title").html();
//        let description=$(this).find(".smaller-card-details").html();
//        let classname=$(this).attr("class");
//        classname=classname.replace(" ",".");
//        $(".accordion .smaller-card").removeClass("Active");
//        $("#leftSide .selectedInfo .left-sub-acronym").html(title);
//        $("#leftSide .selectedInfo .left-sub-title").html(description)
//        changeDynamicLink(appId);
//        $("."+classname).addClass("Active");
//        $(".smaller-card").show();
//        $(".accordion .card").show();
   



//    });




//     $("#rightSide .has-search input").keyup(function(e){
//         let inputVal=$(this).val();
         
         

//        //  $(".search-options-list .option").removeClass("Match");
//        //  $(".search-options-list .option").removeClass("active");
//        //  $(".search-options-list .option").each(function(){$(this).attr("Id",undefined)});
       
//         if(inputVal.length > 0 && e.keyCode != "13"){
//            $(".accordion .card .smaller-card").hide();
//            // console.log( $(".accordion .card .smaller-card").hide());
//             //hide all accordion
//            $(".accordion .card").hide();


//            let count=0;

//             $(".search-options-list .option").each(function(){

//                 let currentOption=$(this);
//                 let title=currentOption.find(".option-title").text();
//                 let description=currentOption.find(".option-details").text();

//                 if(stringMatch(inputVal,title)||stringMatch(inputVal,description)){
//                    currentOption.show();
//                    var optionId= currentOption.attr("data-Id");
//                    let associatedCard=$("."+optionId);
//                    associatedCard.show();
//                    associatedCard.each(function(){$(this).parents(".accordion .card").first().show()})
                    
//                    currentOption.addClass("Match");
//                    currentOption.attr("Id",count+"Match")
//                    count++;
//                 }
//                 else{
//                    currentOption.hide();
//                    // currentOption.removeClass("Match")
//                    // $(".search-options-list .option").each(function(){$(this).attr("Id",null)});
//                 }

//             });

//             if(count>0){
//                // focusedIndex=-1;
//                $(".search-options-list").show();
//             }
//             else{


//                $(".search-options-list").hide();
//             }

//         }
//         else{
//            $(".smaller-card").show();
//            $(".accordion .card").show();
//             $(".search-options-list").hide();
//         }

//     });










//});

 




