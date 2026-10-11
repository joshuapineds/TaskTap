(() => {
  const toast = document.getElementById('toast');
  const rows = document.getElementById('categoryRows');
  let toastTimer;
  let activeTab = 'all';
  let pendingCategoryId = null;
  let nextId = 8;
  const categories = [
    {id:'CAT-001',name:'Cleaning',description:'Household, room, and workspace cleaning tasks.',icon:'✧',color:'indigo',tasks:112,created:'2026-07-14',status:'Active',example:'House cleaning'},
    {id:'CAT-002',name:'Tutoring',description:'Academic support, lessons, and skill coaching.',icon:'✎',color:'teal',tasks:86,created:'2026-07-14',status:'Active',example:'Math tutor'},
    {id:'CAT-003',name:'Repairs',description:'Small home repairs, setup, and maintenance help.',icon:'⌂',color:'amber',tasks:64,created:'2026-07-14',status:'Active',example:'Fix a leaking faucet'},
    {id:'CAT-004',name:'Shopping',description:'Grocery shopping and other local shopping errands.',icon:'▤',color:'purple',tasks:54,created:'2026-07-14',status:'Active',example:'Grocery shopping'},
    {id:'CAT-005',name:'Pet Care',description:'Pet sitting, walking, and basic animal care.',icon:'♡',color:'rose',tasks:51,created:'2026-07-14',status:'Active',example:'Afternoon dog walking'},
    {id:'CAT-006',name:'Delivery',description:'Local pickup and delivery tasks within the community.',icon:'➤',color:'blue',tasks:43,created:'2026-07-14',status:'Active',example:'Prescription pickup'},
    {id:'CAT-007',name:'Others',description:'Community tasks that do not fit another listed category.',icon:'▤',color:'green',tasks:65,created:'2026-07-14',status:'Active',example:'Plant watering'}
  ];
  function notify(message) {
    toast.textContent=message;toast.classList.add('show');clearTimeout(toastTimer);
    toastTimer=setTimeout(()=>toast.classList.remove('show'),2800);
  }
  function escapeHTML(value) {
    return String(value).replace(/[&<>"']/g,character=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[character]));
  }
  function formatDate(value) { return new Date(`${value}T00:00:00`).toLocaleDateString('en-PH',{month:'short',day:'numeric',year:'numeric'}); }
  function getMatches() {
    const query=document.getElementById('categorySearch').value.trim().toLowerCase();
    const sort=document.getElementById('categorySort').value;
    let list=categories.filter(category=>(activeTab==='all'||category.status===activeTab)&&(!query||`${category.name} ${category.description} ${category.example}`.toLowerCase().includes(query)));
    if(sort==='name')list.sort((a,b)=>a.name.localeCompare(b.name));
    if(sort==='popular')list.sort((a,b)=>b.tasks-a.tasks);
    if(sort==='oldest')list.sort((a,b)=>a.created.localeCompare(b.created));
    if(sort==='newest')list.sort((a,b)=>b.created.localeCompare(a.created));
    return list;
  }
  function render() {
    const matches=getMatches();
    const maxTasks=Math.max(1,...categories.map(category=>category.tasks));
    rows.innerHTML=matches.map(category=>{
      const statusClass=category.status.toLowerCase();
      const percent=Math.round((category.tasks/maxTasks)*100);
      return `<tr><td><div class="category-name-cell"><span class="category-icon ${escapeHTML(category.color)}">${escapeHTML(category.icon)}</span><span><strong>${escapeHTML(category.name)}</strong><small>${escapeHTML(category.id)} · Example: ${escapeHTML(category.example)}</small></span></div></td><td class="category-description">${escapeHTML(category.description)}</td><td><div class="usage-bar"><strong class="category-count">${category.tasks}<small>tasks this month</small></strong><span class="usage-track"><i style="width:${percent}%"></i></span></div></td><td class="category-date">${formatDate(category.created)}</td><td><span class="category-state ${statusClass}">${escapeHTML(category.status)}</span></td><td><div class="category-row-actions"><button class="edit-category" data-edit="${escapeHTML(category.id)}" aria-label="Edit ${escapeHTML(category.name)}">Edit</button><button class="category-row-action" data-toggle="${escapeHTML(category.id)}" aria-label="${category.status==='Active'?'Deactivate':'Activate'} ${escapeHTML(category.name)}">${category.status==='Active'?'⊖':'↻'}</button></div></td></tr>`;
    }).join('');
    document.getElementById('categoryEmpty').hidden=matches.length>0;
    document.querySelector('.category-table').hidden=matches.length===0;
    document.getElementById('categoryResult').textContent=`Showing ${matches.length} categor${matches.length===1?'y':'ies'}`;
    document.getElementById('clearCategoryFilters').hidden=!document.getElementById('categorySearch').value;
    document.getElementById('categoryFooterCount').textContent=`${categories.length} sample categor${categories.length===1?'y':'ies'}`;
    const active=categories.filter(category=>category.status==='Active').length;
    const inactive=categories.length-active;
    document.getElementById('totalCategories').textContent=categories.length;
    document.getElementById('activeCategories').textContent=active;
    document.getElementById('inactiveCategories').textContent=inactive;
    document.getElementById('tabAllCategories').textContent=categories.length;
    document.getElementById('tabActiveCategories').textContent=active;
    document.getElementById('tabInactiveCategories').textContent=inactive;
  }
  function openModal(id) { document.getElementById(id).hidden=false; }
  function closeModals() { document.querySelectorAll('.modal-backdrop').forEach(modal=>modal.hidden=true);pendingCategoryId=null; }
  function openEditor(category) {
    const form=document.getElementById('categoryForm');
    form.reset();
    form.elements.id.value=category?.id||'';
    form.elements.name.value=category?.name||'';
    form.elements.description.value=category?.description||'';
    form.elements.icon.value=category?.icon||'✧';
    form.elements.status.value=category?.status||'Active';
    document.getElementById('categoryModalTitle').textContent=category?'Edit category':'Add a category';
    document.getElementById('saveCategoryButton').textContent=category?'Save changes':'Add category';
    openModal('categoryModal');
    form.elements.name.focus();
  }
  function confirmToggle(id) {
    const category=categories.find(item=>item.id===id);
    if(!category)return;
    pendingCategoryId=id;
    const deactivate=category.status==='Active';
    document.getElementById('categoryConfirmTitle').textContent=deactivate?`Deactivate ${category.name}?`:`Activate ${category.name}?`;
    document.getElementById('categoryConfirmCopy').textContent=deactivate?'This category will be marked inactive in this demo. Existing task history is not removed.':'This category will be marked active in this demo.';
    const button=document.getElementById('confirmCategoryAction');
    button.textContent=deactivate?'Deactivate category':'Activate category';
    button.className=deactivate?'button danger-button':'button primary';
    openModal('categoryConfirmModal');
  }
  document.querySelectorAll('.future-link').forEach(link=>link.addEventListener('click',event=>{event.preventDefault();notify(`${link.dataset.page||'This page'} is planned for a future admin milestone.`);}));
  document.querySelectorAll('[data-toast]').forEach(link=>link.addEventListener('click',event=>{event.preventDefault();notify(link.dataset.toast);}));
  ['helpButton','notificationButton','profileButton','adminProfile'].forEach(id=>document.getElementById(id).addEventListener('click',()=>notify(id==='notificationButton'?'No new category alerts in this demo.':'Admin Demo account controls are not connected.')));
  document.getElementById('addCategoryButton').addEventListener('click',()=>openEditor(null));
  document.getElementById('categorySearch').addEventListener('input',render);
  document.getElementById('categorySort').addEventListener('change',render);
  document.getElementById('clearCategoryFilters').addEventListener('click',()=>{document.getElementById('categorySearch').value='';render();});
  document.querySelectorAll('.category-tab').forEach(tab=>tab.addEventListener('click',()=>{activeTab=tab.dataset.status;document.querySelectorAll('.category-tab').forEach(item=>{const selected=item===tab;item.classList.toggle('active',selected);item.setAttribute('aria-selected',selected?'true':'false');});render();}));
  rows.addEventListener('click',event=>{const edit=event.target.closest('[data-edit]');const toggle=event.target.closest('[data-toggle]');if(edit)openEditor(categories.find(category=>category.id===edit.dataset.edit));if(toggle)confirmToggle(toggle.dataset.toggle);});
  document.getElementById('categoryForm').addEventListener('submit',event=>{
    event.preventDefault();
    const form=event.currentTarget;
    const id=form.elements.id.value;
    const name=form.elements.name.value.trim();
    const duplicate=categories.some(category=>category.name.toLowerCase()===name.toLowerCase()&&category.id!==id);
    if(duplicate){form.elements.name.setCustomValidity('A category with this name already exists.');form.elements.name.reportValidity();return;}
    form.elements.name.setCustomValidity('');
    const values={name,description:form.elements.description.value.trim(),icon:form.elements.icon.value,status:form.elements.status.value};
    if(id){const category=categories.find(item=>item.id===id);Object.assign(category,values);notify(`${name} updated in this demo view.`);}
    else{const color=['indigo','teal','amber','purple','blue','green','rose'][categories.length%7];categories.push({id:`CAT-${String(nextId++).padStart(3,'0')}`,...values,color,tasks:0,created:'2026-10-11',example:'New tasks'});notify(`${name} added to this demo view.`);}
    closeModals();render();
  });
  document.querySelector('#categoryForm [name="name"]').addEventListener('input',event=>event.currentTarget.setCustomValidity(''));
  document.getElementById('confirmCategoryAction').addEventListener('click',()=>{
    const category=categories.find(item=>item.id===pendingCategoryId);
    if(!category)return;
    category.status=category.status==='Active'?'Inactive':'Active';
    const name=category.name;
    closeModals();render();notify(`${name} is now ${category.status.toLowerCase()} in this demo.`);
  });
  document.querySelectorAll('[data-close-modal]').forEach(button=>button.addEventListener('click',closeModals));
  document.querySelectorAll('.modal-backdrop').forEach(modal=>modal.addEventListener('click',event=>{if(event.target===modal)closeModals();}));
  document.addEventListener('keydown',event=>{if(event.key==='Escape')closeModals();if((event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='k'){event.preventDefault();document.getElementById('categorySearch').focus();}});
  const sidebar=document.getElementById('sidebar');
  const scrim=document.getElementById('sidebarScrim');
  function closeSidebar(){sidebar.classList.remove('open');scrim.classList.remove('open');}
  document.getElementById('mobileMenu').addEventListener('click',()=>{sidebar.classList.toggle('open');scrim.classList.toggle('open');});
  scrim.addEventListener('click',closeSidebar);
  render();
})();
