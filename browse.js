const $=(s,r=document)=>r.querySelector(s),$$=(s,r=document)=>[...r.querySelectorAll(s)];
const toast=m=>{const e=$("#toast");e.textContent=m;e.classList.add("show");clearTimeout(window.tt);window.tt=setTimeout(()=>e.classList.remove("show"),2400)};
const modal=$("#modal"),auth=$("#auth");

const data={
"House Cleaning":{price:"₱500",location:"Brgy. 176",deadline:"Oct 7, 2026",text:"General house cleaning assistance for a local resident. The exact address is protected and should only be shared at the appropriate stage of the task."},
"Math Tutor":{price:"₱300",location:"Brgy. 178",deadline:"Oct 10, 2026",text:"Math tutoring for a Grade 10 learner. The task can later connect to request management and in-app messaging."},
"Pet Care":{price:"₱400",location:"Brgy. 176",deadline:"Oct 12, 2026",text:"Pet care assistance within the local community. Guests can see public task information without accessing private user details."},
"Fix leaking faucet":{price:"₱600",location:"Brgy. 176",deadline:"Oct 8, 2026",text:"Basic plumbing assistance for a leaking faucet. The requirements can be expanded on the full task details page."},
"Grocery Shopping":{price:"₱450",location:"Brgy. 179",deadline:"Oct 9, 2026",text:"Local grocery shopping and delivery task. The platform can later track requests and task status."},
"Apartment Cleaning":{price:"₱550",location:"Brgy. 178",deadline:"Oct 11, 2026",text:"One-time apartment cleaning before the weekend."}
};

function openAuth(mode="login"){auth.hidden=false;setAuth(mode);document.body.style.overflow="hidden"}
function setAuth(mode){$$(".tab").forEach(x=>x.classList.toggle("active",x.dataset.tab===mode));$("#login").hidden=mode!=="login";$("#register").hidden=mode!=="register"}
function closeAll(){modal.hidden=true;auth.hidden=true;document.body.style.overflow=""}
$$("[data-auth]").forEach(b=>b.addEventListener("click",()=>openAuth(b.dataset.auth)));
$("#close").onclick=()=>{modal.hidden=true;document.body.style.overflow=""};
$("#authClose").onclick=closeAll;
[modal,auth].forEach(m=>m.addEventListener("click",e=>{if(e.target===m)closeAll()}));
$$(".tab").forEach(t=>t.onclick=()=>setAuth(t.dataset.tab));
document.addEventListener("keydown",e=>{if(e.key==="Escape")closeAll()});

$$(".view-task").forEach(b=>b.onclick=()=>{
  const task=encodeURIComponent(b.dataset.task);
  window.location.href="task-details.html?task="+task;
});
$("#modalReport").onclick=()=>toast("Reporting will require an authenticated account in the full system.");

$$(".save").forEach(b=>b.onclick=()=>{b.classList.toggle("saved");b.textContent=b.classList.contains("saved")?"♥":"♡";toast(b.classList.contains("saved")?"Task saved.":"Task removed from saved tasks.")});

const cards=$$(".task-item"), list=$("#taskList"), empty=$("#empty");
function apply(){
  const q=$("#search").value.trim().toLowerCase();
  const br=$("#filterBarangay").value;
  const topBr=$("#barangay").value;
  const min=Number($("#minPrice").value||0), max=Number($("#maxPrice").value||Infinity);
  const deadline=$("#deadline").value;
  const cats=$$(".check-row input:checked").map(x=>x.value);
  let count=0;
  cards.forEach(c=>{
    const title=c.dataset.title.toLowerCase(), category=c.dataset.category, price=Number(c.dataset.price), barangay=c.dataset.barangay, days=Number(c.dataset.days);
    const okQ=!q||title.includes(q)||category.toLowerCase().includes(q);
    const okCat=!cats.length||cats.includes(category);
    const selectedBr=br!=="all"?br:topBr;
    const okBr=selectedBr==="all"||barangay===selectedBr;
    const okPrice=price>=min&&price<=max;
    const okDeadline=deadline==="all"||(deadline==="soon"&&days<=3)||(deadline==="week"&&days<=7);
    c.hidden=!(okQ&&okCat&&okBr&&okPrice&&okDeadline);
    if(!c.hidden)count++;
  });
  $("#resultCount").textContent=count;empty.hidden=count!==0;
}
$("#applyFilters").onclick=()=>{apply();toast("Filters applied.")};
$("#applySearch").onclick=()=>{apply();toast("Search updated.")};
$("#search").addEventListener("input",apply);
$("#barangay").addEventListener("change",()=>{$("#filterBarangay").value=$("#barangay").value;apply()});
$("#reset").onclick=$("#emptyReset").onclick=()=>{
  $("#search").value="";$("#barangay").value="all";$("#filterBarangay").value="all";$("#minPrice").value="";$("#maxPrice").value="";$("#deadline").value="all";$$(".check-row input").forEach(x=>x.checked=false);apply();toast("Filters reset.");
};
$("#sort").addEventListener("change",()=>{
  const mode=$("#sort").value;
  const sorted=[...cards].sort((a,b)=>{
    if(mode==="price-low")return +a.dataset.price-+b.dataset.price;
    if(mode==="price-high")return +b.dataset.price-+a.dataset.price;
    if(mode==="deadline")return +a.dataset.days-+b.dataset.days;
    return 0;
  });
  sorted.forEach(x=>list.appendChild(x));apply();
});
$$(".view-toggle").forEach((b,i)=>b.onclick=()=>{
  $$(".view-toggle").forEach(x=>x.classList.remove("active"));b.classList.add("active");
  if(i===1){list.style.gridTemplateColumns="repeat(2,minmax(0,1fr))";cards.forEach(c=>{c.style.gridTemplateColumns="1fr";c.querySelector(".view-task").style.width="100%"})}
  else{list.style.gridTemplateColumns="1fr";cards.forEach(c=>{c.style.gridTemplateColumns="115px 1fr 90px";c.querySelector(".view-task").style.width=""})}
});
$("#menuBtn").onclick=()=>toast("Mobile navigation: connect this control to your preferred menu pattern.");
$("#loginForm").onsubmit=e=>{e.preventDefault();closeAll();toast("Demo login submitted.")};
$("#registerForm").onsubmit=e=>{e.preventDefault();closeAll();toast("Demo registration submitted.")};
apply();
