$('#phone').find('a').click(
function(){var
link,phoneKey=window.item_phone||'';

function phoneDemixer(key){var
pre=key.match(/[0-9a-f]+/g),mixed=(item.id%2===0?pre.reverse():pre).join(''),s=mixed.length,r='',k;for(k=0;k<s;++k){if(k%3===0){r+=mixed.substring(k,k+1);}}
return r;}

if(!phoneKey){return;}
link='/items/phone/'+item.url+'?pkey='+phoneDemixer(phoneKey);
$('#phone .phone').html('<img src="'+static_prefix+'/s/a/i/ic/ajax-loader.gif" class="loader" /> Идет загрузка...');

$('#phone .alert').show();
window.setTimeout(function(){$('#phone .phone').html('<img src="'+link+'" />');},1000);