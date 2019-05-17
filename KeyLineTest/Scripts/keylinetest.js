var chart;
var showNodes = null, queryStr = null; //filter
var clicks = 0, timer = null; //dblclick
var getPhoneOwner;//contentmemu

//取得選取顏色
function getSelectionColour() {
    return 'rgb(255,195 , 0)';
}

//取得layout名稱
function getLayout() {
    return $("[name=layout]:checked").val();
}
function doLayout() {
    var layout = getLayout();
    
    chart.layout(layout, { fit: true, tidy: true, animate: true, level: 'level' });
}

//連點單點handler
function clickHandler(id) {
    
   //區分連點和單點
    clicks++;
    if (clicks === 1) {

        //single click
        timer = setTimeout(function () {
            clicks = 0;

            //filter
            showNodes = null;
            setSelection(id);

            if (id === null) {
                applyFilters();
            } else {

                applyFilterAndLayout();
            }

        }, 400);
    } else {
        //double click
        clearTimeout(timer);
        clicks = 0;

        //資料庫抓資料
        $('#url').val('/Home/ExtendNode')
        expandNode(id);
    }
}

//顯示popover
function hoverTooltip(id) {
    if (id) {
        var item = chart.getItem(id);

        if (item.type === 'link' && item.d.ld.length > 0) {
            

            //取得線中心的位置
            var node1 = chart.getItem(item.id1), node2 = chart.getItem(item.id2);
            var node1Pos = chart.viewCoordinates(node1.x, node1.y), node2Pos = chart.viewCoordinates(node2.x, node2.y);
            //var coordinates = chart.viewCoordinates(Math.abs(node1.x + node2.x) / 2, Math.abs(node1.y + node2.y) / 2);
            var x = (node1Pos.x + node2Pos.x )/2;
            var y = (node1Pos.y + node2Pos.y) / 2;
            x = x + $('#kl').position().left;
            y = y + $('#kl').position().top;

            ///popover的title content
            var tooltipTitle = $('.tooltipTitle').find('strong').clone(true,true).append(item.id);//"<strong>" + item.id + "</strong>";
            var tooltipLevel1 ;
            if (item.d) {
                //tooltipLevel1 = document.createElement('table');

                //$(tooltipLevel1).css('width', '100%').append("<tbody></tbody>")
                //.append("<tr> <td style= 'text-align:right' class='col-md-5'> <strong>時間</strong></td> <td>" + item.d.ld[0].dt + "</td> </tr >")
                //    .append("<tr> <td style= 'text-align:right'  class='col-md-5'> <strong>通話類型</strong></td> <td>" + item.d.ld[0].ct + "</td> </tr >")
                //    .append("<tr> <td style= 'text-align:right'  class='col-md-5'> <strong>IMEI</strong></td> <td>" + item.d.ld[0].im + "</td> </tr >")
                //    .append("<tr> <td style= 'text-align:right'  class='col-md-5'> <strong>基地台位置</strong></td> <td>" + item.d.ld[0].bs + "</td> </tr >");
                ///////test
                popoverReturn = function () {
                    $('div.popover-content').html(tooltipLevel1);
                }

                popoverBtnClick = function (that) {
                    
                    var linkContent = document.createElement('table');

                    $(linkContent).append("<tbody></tbody>")
                    var linkContentHeader = ['時間', '通話時間', '通話類型', 'IMEI', '基地台位置'];
                    
                    $('div.popover-content').html('<div class="popover-content-inside"></div>').children('div')
                        .append('<button type="button" onclick="popoverReturn()" class="btn btn-default" style="width:100%"><i class="glyphicon glyphicon-chevron-left"></i>返回</button>')
                        .append(linkContent);
                    var linkContentData = item.d.ld.filter(x => x.dt === $(that).data('linkid'))[0];
                    Object.keys(linkContentData).map(function (key, i) {
                        $('div.popover-content').find('tbody').append('<tr> <td style="text- align:right" class="col-md-5"> <strong>' + linkContentHeader[i] + '</strong></td> <td>' + linkContentData[key]+'</td> </tr>')
                    })
                    
                    

                }
                tooltipLevel1 = document.createElement('div');
                $(tooltipLevel1).addClass('list-group').addClass('popover-content-inside');
                var linkdata='';
                item.d.ld.map(function (ldItem) {
                    linkdata += ('<button type="button" class="list-group-item" data-linkid="' + ldItem.dt + '" onclick="popoverBtnClick(this)" style="width:100%">' + ldItem.dt + '<i class="glyphicon glyphicon-chevron-right"></i></button>')
                });
                $(tooltipLevel1).append(linkdata);
                
                
                
                    ///test
            }

            //更改popover內容要先destroy
            $('#tooltipHidden').popover('destroy');
            setTimeout(function () {    //因為destroy是異步，所以給timeout再修改
                $('#tooltipHidden').css({ 'top': y, 'left': x })
                    .popover({
                        html: true,
                        trigger: 'hover',
                        placement: 'top',
                        title: function () {
                            return tooltipTitle;
                        },
                        content: function () {
                            return tooltipLevel1;
                        }
                    })
                $('#tooltipHidden').popover('show'); //popover show
            }, 200);
            

            
        } else {
            $('#tooltipHidden').popover('hide');
        }
    } else {
        $('#tooltipHidden').popover('hide');
    }
}

//顯示menuContext
function showMenuContext(item, x, y) {
    //關掉預設contextMenu
    $('#contextMenuNode').on('contextmenu', function (event) {
        event.preventDefault();
        return false;
    })
    
    if (item && item.type === 'node') {
        if (item.d.ct) {

            //取得電話持有人
            getPhoneOwner = function () {
                $('#url').val('/Home/GetPhoneOwner');
                expandNode(item.id);
                hideMenuContext();
            }

            //取得電話
            getPhone = function () {
                $('#url').val('/Home/GetPhone');
                expandNode(item.id);
                hideMenuContext();
            }

            //根據點的性質出現不同的按鈕
            if (item.d.ct === 'phone') {
                $('#getPhoneOwner').show();
                $('#getPhoneOwner').on('click', getPhoneOwner);
            }else if ( item.d.ct === 'person') {
                $('#getPhone').show();
                $('#getPhone').on('click',getPhone);
            }

            //MenuContext移到點上
            x = x + $('#kl').position().left - 10;  //-10 讓memu剛好在滑鼠上
            y = y + $('#kl').position().top - 10;
            $('#contextMenuNode').css({ 'top': y, 'left': x }).show();
        }
    }
}

//關閉MenuContext
function hideMenuContext() {

    //註銷menu上 click event
    $('#getPhoneOwner').off('click', getPhoneOwner);
    $('#getPhone').off('click', getPhone);

    $('#getPhoneOwner').hide();
    $('#getPhone').hide();
    $('#contextMenuNode').hide();
}

//資料庫撈資料增加node
function expandNode(id) {
    var item = chart.getItem(id);
    if (item && item.type === 'node') {
        var options = {
            animate: true,
            layout: {
                name: getLayout,
                fix: 'all', //所有點固定，不重新排版
            },
            //expand也可以加入filter
            //filter: {   
            //    filterFn: filter,
            //    type:'node'
            //}
        };

        //要求此id通聯紀錄
        $.ajax({
            type: 'post',
            url: $("#url").val(),
            data: {
                queryStr: id
            },
            success: function (result) {

                data = dataProcess(result);
                console.log(data);

                
                chart.expand(data, options,
                    function () {

                        
                        showNodes = null;
                        setSelection(id);
                        applyFilterAndLayout();

                        //畫面移到新nodes上
                        var zoomNodes = data.items
                            .filter(function (item, index, array) { return item.type === 'node'; })
                            .map(function (e) { return e['id'] });;
                        chart.zoom('fit', { animate: true, ids: zoomNodes ,time:500})
                    });
                
            },
            error: function (err) {
                console.log(err);
            }

        });
        

        
    }
}

//和id有關聯的node才能通過filter
function setSelection(id) {
    if (id === null) {
        //若沒有id，全部show
        showNodes = null;
        
    } else {
        //若有id，show出關聯點
        var item = chart.getItem(id);
        if (item && item.type === 'node') {

            //parent關聯點
            if (showNodes) {
                Object.assign(showNodes, chart.graph().distances(id, { direction: 'to' }));
            } else {
                showNodes = chart.graph().distances(id, { direction: 'to' });
            }
            //child關聯點，一層
            var neighbour = chart.graph().neighbours(id, { direction: 'from', all: true });
            //將array轉成object
            var neighbourChild = neighbour.nodes.reduce(function (o, key) {
                o[key] = 1;
                return o;
            }
            , {});
            
            Object.assign(showNodes, neighbourChild);
            
        }
    }
}

//filter規則
function filter(item) {
    if (item.type === 'node') {
        if (showNodes) {
            return showNodes[item.id] !== undefined;
        }
        if (queryStr) {
            return item.id === queryStr;
        }
        return true;
    }
    return true;
    //return true可以顯示
}

//套用filter
function applyFilters(filterCallback) {
    //套用filter
    chart.filter(
        filter,     //filter規則
        { time: 300 },  //option
        function (result) {
            if (filterCallback) {
                filterCallback(result);
            }

            
        }
    );

        
}

//套用filter + 重新排版
function applyFilterAndLayout() {
    applyFilters(function (result) {
       
        if (result.shown.nodes.length > 0 || result.hidden.nodes.length > 0) {
            doLayout();
        }
    });
}

//處理data
function dataProcess(res ) {

    
    var width = $('#kl').width();
    var height = $('#kl').height();
    var dataTmp;
    //處理icon
    dataTmp = JSON.parse(res, function (key, value) {
        //font icon 使用keyline js方法取值
        if (key === 'fi') {
            value.t = eval(value.t);
        }
        
        return value;
    });

    //相反的link id 換成相同的 
    if (chart) {
        dataTmp.items = dataTmp.items.map(function (item) {
            if (item.type === 'link') {
                var reverseId = item.id.split('-');
                reverseId = reverseId.reverse().join('-');
                var existedLink = chart.getItem(reverseId);
                if (existedLink && existedLink.type === 'link') {
                    item.id = reverseId;
                    item.id1 = existedLink.id1;
                    item.id2 = existedLink.id2;
                }
            }
            return item;
        })
    }
    

    ////點數過多，給定位置
    if (dataTmp.items.length > 30000) {
        dataTmp.items = dataTmp.items.map(function (item) {
            if (item.type === 'node' ) {
                let xRnd = Math.floor(Math.random() * width), yRnd = Math.floor(Math.random() * height);
                return Object.assign({}, item, {
                    x: Math.floor(Math.random() * width),
                    y: Math.floor(Math.random() * height)
            
                })
            }
            return item;
        })
    }
    


    return dataTmp;
}

//在chart塞入資料
function klReady(err, loadedChart) {    //若有err，會有錯誤訊息
    chart = loadedChart;

    //chart設定參數
    var options = {
        linkEnd: {
            avoidLabel: true,
            space: 'loose'
        },
        selectionColour: getSelectionColour(),
        handMode: true,
        //fontFamily: '微軟正黑體,helvetica'


    };
    chart.options(options);

    //chart匯入item
    chart.load(data, function () {

        if (data.items.length < 30000) {
            //30000以下的點，才能自動排版
            doLayout();
            chart.zoom('fit');
        }
    });

    //點擊背景或是node
    chart.bind('click', clickHandler);
    //popover
    chart.bind('hover', hoverTooltip);
    //按右鍵
    chart.bind('contextmenu', function (id, x, y) {
        event.preventDefault();
        var item = chart.getItem(id);
        hideMenuContext();
        showMenuContext(item, x, y);
    });
}

//啟用Keylines
function startKeyLines() {
    //assets folder是Keylines 資源資料夾，一定在create之前設定path，
    KeyLines.paths({ assets: "../assets/" });
    var options = {
        iconFontFamily: 'Font Awesome\ 5 Free' //第5版的font awesome
    };
    //建立keyline
    //將div轉換成Chart 或是 Timebar 傳到callback
    KeyLines.create({ id: 'kl', options: options }, klReady);
}

//Webfont 載入font，啟用KeyLines
function WebFontKeyLines() {
    if (chart) {
        //chart存在，merge data
        chart.merge(data, doLayout);
    } else {
        //chart不存在，啟用Keylines
        WebFont.load({
            custom: {
                families: ['Font Awesome\ 5 Free']
            },
            google: {
                families: ['Droid Sans', 'Droid Serif']
            },
            testString: {
                'Font Awesome\ 5 Free': '\uf00c\uf000'
            },
            active: startKeyLines,
            inactive: startKeyLines,
            timeout: 3000
        });
    }
}

$(function () {

    //test測試用
    data = {
        type: 'LinkChart',
        items: [{
            id: 'id1',
            t: '某人',
            type: 'node',
            c: '#50B39A',
            u: "/Image/person.png",
            fi: { t: KeyLines.getFontIcon('fa-user-circle') }
        }, {
            id: 'id2',
            t: '電話',
            type: 'node',
            c:'#B8E62E',
            u: "/Image/phone.png",
            fi: { t: KeyLines.getFontIcon('fa-user-circle') }

        }, {
            id: 'id1-id2',
            id1: 'id1',
            id2: 'id2',
            type: 'link',
            a2: true,
            w: 2,
            c: '#E1F230',
            d: {
                ld: 
                    [
                        { dt: "2012-11-11 23:59:59", pr: "244", ct: "收話", im: "358883048805640", bs: "713125367/台中市西區向上路一段_462號" },
                        { dt: "2012-11-20 15:29:26", pr: "93", ct: "發話", im: "358883048805640", bs: "713125367/台中市西區向上路一段_462號" }

                    ]
                
            
            }
            
        }]
    };

    WebFontKeyLines();
    

    //排版樣式按鈕
    $('[name=layout]').click(doLayout);

    //自動排版按鈕
    $("#adjacent").click(doLayout);

    //階層排版 隱藏自動排版按鈕
    $("input[type=radio]").change(function () {
        if ($(this).val() === 'standard') {
            $('#adjacent').show('slow');
        } else {
            $('#adjacent').hide('slow');
        }


    })

    //搜尋filter按鈕
    $(".search [role=button]").click(function () {

        //查詢字串
        queryStr = $(".search [type=text]").val();
        let changes = [];
        showNodes = null;
        chart.each({ type: 'node' }, function (node) {
            //把有選取的圈圈刪掉
            if (node.ha2) {
                changes.push({
                    id: node.id,
                    ha2: null
                });
            }
            //符合字串的加上圈圈
            if (queryStr && node.id.includes(queryStr)) {
                changes.push({
                    id: node.id,
                    ha2: {
                        c: 'rgb(2, 44, 53)',
                        w: 8,
                        r: 37
                    }
                });
                //符合字串的關聯點show出來
                setSelection(node.id);
            }
        });

        if (queryStr === null) {
            //空查詢全部show出來
            applyFilters();
        } else {
            //有符合字串的關聯點套用filter
            chart.setProperties(changes);
            applyFilterAndLayout();
        }
        queryStr = null;
    });
    
    //列印按鈕
    $("#exportBtn").click(function () {
        var width = $('#kl').width();   
        var height = $('#kl').height();
        
        chart.toDataURL(width, height, {
            noScale: true,
            fit: 'oneToOne',

        }, function (dataURL) {
            var tab = window.open();
            tab.document.write("<html ><head> ")
            tab.document.write("<style type='text/css' media='print'> @media print{html, body, img { width:100%;}}</style></head> ")
            tab.document.write("<body onload='window.print(); '>")
            tab.document.write("<img src ='" + dataURL + "' crossOrigin='anonymous'/>");
            tab.document.write("</body></html>")
            tab.document.close();
            
        });


        
    })

    //測試按鈕
    $('#test').click(function () {
        $('#url').val('/Home/GetPhoneOwner');
        expandNode('0931433533(台哥大)');
        console.log('test',data);
    })

    //滑鼠移開後關閉menuContext
    $('#contextMenuNode').mouseleave(function () {
        hideMenuContext();
    })
    
});