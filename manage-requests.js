(() => {
  const $ = s => document.querySelector(s);
  const $$ = s => [...document.querySelectorAll(s)];
  const toast = msg => { const el=$('#toast'); if(!el)return; el.textContent=msg; el.classList.add('show'); clearTimeout(window.__manageToast); window.__manageToast=setTimeout(()=>el.classList.remove('show'),2400); };
  const cards=$$('.request-card'); let filter='all', query='', sort='recommended', selected=null;
  const apply=()=>{
    const visible=cards.filter(c=>{
      const state=c.dataset.state, text=c.dataset.search||'';
      const stateOk=filter==='all'||state===filter;
      const searchOk=!query||text.includes(query);
      c.hidden=!(stateOk&&searchOk); return stateOk&&searchOk;
    });
    const list=$('#requestList');
    [...visible].sort((a,b)=> sort==='rating' ? Number(b.dataset.rating)-Number(a.dataset.rating) : sort==='recent' ? new Date(b.dataset.date)-new Date(a.dataset.date) : (a.dataset.state==='shortlisted'? -1:1)).forEach(c=>list.appendChild(c));
    $('#requestCount').textContent=`${visible.length} request${visible.length===1?'':'s'}`;
    $('#emptyState').hidden=visible.length!==0;
  };
  $$('.filter-chip').forEach(btn=>btn.addEventListener('click',()=>{filter=btn.dataset.filter;$$('.filter-chip').forEach(b=>b.classList.toggle('active',b===btn));apply();}));
  $('#requestSearch')?.addEventListener('input',e=>{query=e.target.value.trim().toLowerCase();apply();});
  $('#requestSort')?.addEventListener('change',e=>{sort=e.target.value;apply();});
  $('#clearFilters')?.addEventListener('click',()=>{$('#requestSearch').value='';query='';filter='all';$$('.filter-chip').forEach((b,i)=>b.classList.toggle('active',i===0));apply();});
  const updateSelection=card=>{
    selected=card; cards.forEach(c=>c.classList.toggle('selected',c===card));
    const avatar=card.querySelector('.applicant-avatar'), name=card.querySelector('h2')?.textContent||'', rating=card.dataset.rating;
    $('#selectedBox').innerHTML=`<div class="selected-person"><div class="selected-person-head"><div class="applicant-avatar ${avatar.className.split(' ').slice(1).join(' ')}">${avatar.textContent}</div><div><strong>${name}</strong><small>${rating} ★ · ready to start</small></div></div><p>This applicant will be selected as the performer. You can still message them before confirming.</p><button class="text-action" id="clearSelection">Clear selection</button></div>`;
    $('#acceptSelected').disabled=false; $('#clearSelection').addEventListener('click',clearSelection);
  };
  const clearSelection=()=>{selected=null;cards.forEach(c=>c.classList.remove('selected'));$('#selectedBox').innerHTML='<div class="selected-empty"><span>◎</span><strong>No one selected yet</strong><small>Shortlist applicants or accept a request below.</small></div>';$('#acceptSelected').disabled=true;};
  $$('.request-card').forEach(card=>{
    card.querySelector('.shortlist-btn')?.addEventListener('click',()=>{
      const now=card.dataset.state!=='shortlisted'; card.dataset.state=now?'shortlisted':'new'; card.classList.toggle('shortlisted',now);
      const badge=card.querySelector('.request-badge'); badge.textContent=now?'Shortlisted':'New request'; badge.className=`request-badge ${now?'shortlisted':'new'}`; toast(now?'Applicant shortlisted.':'Applicant removed from shortlist.'); apply();
    });
    card.querySelector('.message-btn')?.addEventListener('click',()=>toast(`Opening a message with ${card.querySelector('h2').textContent}…`));
    card.querySelector('.view-profile')?.addEventListener('click',()=>toast(`Opening ${card.querySelector('h2').textContent}'s profile…`));
    card.querySelector('.decline-btn')?.addEventListener('click',()=>{card.remove();toast('Request declined.');});
    card.addEventListener('dblclick',()=>updateSelection(card));
  });
  $('#acceptSelected')?.addEventListener('click',()=>{if(!selected)return;const name=selected.querySelector('h2').textContent;$('#decisionSummary').innerHTML=`<strong>${name}</strong><br><span>${selected.dataset.rating} ★ · ${selected.querySelector('.request-meta')?.textContent||'Applicant'}</span>`;$('#decisionModal').hidden=false;$('#confirmDecision').dataset.name=name;});
  const closeModal=()=>$('#decisionModal').hidden=true;
  $('#closeDecision')?.addEventListener('click',closeModal);$('#cancelDecision')?.addEventListener('click',closeModal);$('#decisionModal')?.addEventListener('click',e=>{if(e.target.id==='decisionModal')closeModal();});
  $('#confirmDecision')?.addEventListener('click',()=>{const name=$('#confirmDecision').dataset.name;closeModal();toast(`${name} accepted. Task is now assigned.`);$('#acceptSelected').disabled=true;$('#acceptSelected').textContent='Performer selected';selected?.classList.add('accepted');});
  $('#compareBtn')?.addEventListener('click',()=>{const n=cards.filter(c=>c.dataset.state==='shortlisted'&&!c.hidden).length;toast(n?`Comparing ${n} shortlisted applicant${n===1?'':'s'}…`:'Shortlist applicants to compare them.');});
  apply();
})();
