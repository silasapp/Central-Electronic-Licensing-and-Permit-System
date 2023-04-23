
$(function () {

    UpdateHeader();
    LoadLicenses();

    $('a[href="#"]').click(function (e) {
        e.preventDefault();
    });

    $(window).scroll(function () {
        UpdateHeader();
    });

    $(".input-control.alt").each(function () {
        var w = $(this).find("button").outerWidth();
        $(this).find("input").css("padding-left", (parseInt(w) + 5) + "px");
    });

    $('.sidebar').on('click', 'li', function () {
        if (!$(this).hasClass('active')) {
            $('.sidebar li').removeClass('active');
            $(this).addClass('active');
        }
    });



    $(document.body).on("change", ".countrySelect", function (e) {
        var c = $(this).val();
        var State2Feed = '#' + $(this).attr('data-state');
        //alert(c);
        var busyBox = $('.stateToFeedLabel');
        busyBox.append(LoadingVSmall_left());

        $.getJSON('/company/GetState/'+c,  function (states) {
           // alert(states);
            //{ id: c },
            var StatesSelect = $(State2Feed);
            busyBox.find("span.busy").remove();
            StatesSelect.empty();
            StatesSelect.append($('<option/>', { Value: "", text: "Select State" }));
            $.each(states, function (index, itemData) {
                
                StatesSelect.append(
                    
                    $('<option/>', { value: itemData.Id, text: itemData.Name }));

            });

        });
        e.preventDefault();
    });

    $(document.body).on("change", "#license", function (e) {
        var c = $(this).val();
        
        //alert(c);
        
        $.getJSON('/Application/getCategory/' + c, function (cats) {
            // alert(states);
            //{ id: c },
            var catSelect = $('#category');
            
            catSelect.empty();
            catSelect.append($('<option/>', { Value: "", text: "Select Category" }));
            catSelect.append($('<option/>', { Value: "", text: "All" }));
            //<option value="">All</option>
            $.each(cats, function (index, itemData) {

                catSelect.append(

                    $('<option/>', { value: itemData.Name, text: itemData.Name }));

            });

        });
        e.preventDefault();
    });


    function UpdateHeader() {
        if ($(document).scrollTop() >= 60) {
            $("#navbar").addClass("navbar-fixed");
            //$(".miniLogo").show(300);
        }
        else {
            $("#navbar").removeClass("navbar-fixed");
            //$(".miniLogo").hide(300);
        }
    }

    function LoadLicenses() {
        var url = "/Dashboard/GetLicenses";
        $.get(url, function (data) {
            var licSelect = $('#lstLicenses');

            licSelect.empty();
            $.each(data, function (index, itemData) {
                //var toAdd = '<li class=""><a href="/License/ViewLicense/' + itemData.Id + '">' + itemData.ShortName + '</a></li>';
                var toAdd = '<li class=""><a href="/Account/ProcessAppData?q=' + itemData.publicKey + '">' + itemData.ShortName + '</a></li>';
                licSelect.append(toAdd);
            });
        });
    }

    function pushMessage(t) {
        var mes = 'Info|Implement independently';
        $.Notify({
            caption: mes.split("|")[0],
            content: mes.split("|")[1],
            type: t
        });
    }

    function showDialog(id) {
        var dialog = $(id).data('dialog');
        dialog.open();
    }

    function Busy() {
        var busy = '<div class="sk-fading-circle">'
          + '<div class="sk-circle1 sk-circle"></div><div class="sk-circle2 sk-circle"></div>'
          + '<div class="sk-circle3 sk-circle"></div><div class="sk-circle4 sk-circle"></div>'
          + '<div class="sk-circle5 sk-circle"></div><div class="sk-circle6 sk-circle"></div>'
          + '<div class="sk-circle7 sk-circle"></div><div class="sk-circle8 sk-circle"></div>'
          + '<div class="sk-circle9 sk-circle"></div><div class="sk-circle10 sk-circle"></div>'
          + '<div class="sk-circle11 sk-circle"></div><div class="sk-circle12 sk-circle"></div>'
          + '</div>';

        return busy;
    }

    function Loading() {
        var loading = '<div class="busy"><img src="/Content/Images/loading.gif" /></div>';
        return loading;
    }

    function LoadingSmall() {
        var loading = '<div class="busy"><br /><img style="width: 25px;" src="/Content/Images/loading.gif" /></div>';
        return loading;
    }

    function LoadingVSmall() {
        var loading = '<div class="busy txtcenter"><img style="width: 15px;" src="/Content/Images/loading.gif" /></div>';
        return loading;
    }

    function LoadingVSmall_left() {
        var loading = '<span class="busy"><img style="width: 15px;" src="/Content/Images/loading.gif" /></span>';
        return loading;
    }

    
});

function NotifyUser(msg, title, typ) {
    setTimeout(function () {
        $.Notify({ timeout: 10000, type: typ.length > 0 ? typ : 'default', caption: title.length > 0 ? title : 'Alert!', content: msg.length > 0 ? msg : "Alert! Alert!! Alert!!!" });
    }, 700);
}