(function(){
  const $=s=>document.querySelector(s), $$=s=>[...document.querySelectorAll(s)];
  const data={
    juan:{name:'Jovan Floidazamie',initials:'JF',avatar:'blue',presence:'Online now',task:'Fix a leaking faucet',taskId:'fix-leaking-faucet',banner:'₱600 · Repairs · Brgy. 176 · Due Oct 8, 2026',status:'Pending',messages:[['day','Today'],['them','Hi Joshua, I saw your request for the faucet task. Can you confirm you have basic plumbing tools?','2:41 PM'],['mine','Yes, I have an adjustable wrench, screwdrivers, and plumber’s tape. I can also check the connection under the sink.','2:44 PM'],['them','Perfect. Would 3 PM tomorrow work for you?','3:02 PM'],['mine','Yes, 3 PM works. I’ll message you here if anything changes.','3:04 PM'],['them','Great. Please message me when you’re on the way.','3:07 PM']]},
    maria:{name:'Sean Dreza',initials:'SD',avatar:'orange',presence:'Active 1h ago',task:'Math Tutor (Grade 10)',taskId:'math-tutor',banner:'₱300 · Tutoring · Brgy. 178 · Due Oct 10, 2026',status:'Accepted',messages:[['day','Yesterday'],['them','Hi Joshua! Thanks for offering to tutor. What time are you available Saturday?','10:11 AM'],['mine','I’m available from 9 AM to 12 PM. I can focus on algebra and geometry review.','10:18 AM'],['them','Thanks! Saturday morning works for me.','10:22 AM']]},
    andre:{name:'Zef Ian Junio',initials:'ZJ',avatar:'green',presence:'Active yesterday',task:'House Cleaning',taskId:'house-cleaning',banner:'₱500 · Cleaning · Brgy. 176 · Due Oct 7, 2026',status:'In progress',messages:[['day','Oct 5'],['them','I left the checklist in the task details.','4:30 PM'],['mine','Got it. I’ll follow the checklist and send an update when I’m finished.','4:42 PM']]},
    liza:{name:'Liza Cruz',initials:'LC',avatar:'purple',presence:'Active Oct 4',task:'Pet Care',taskId:'pet-care',banner:'₱400 · Pet Care · Brgy. 176 · Due Oct 12, 2026',status:'Pending',messages:[['day','Oct 4'],['them','Could you confirm the feeding instructions?','9:15 AM'],['mine','Sure. I’ll follow the instructions in the task details and confirm anything unclear before starting.','9:27 AM']]}
  };
  const workspace=$('.message-workspace'), list=$('#conversationList'), empty=$('#conversationEmpty'), search=$('#conversationSearch'), messages=$('#chatMessages');
  let current='juan', filter='all';
  function avatarMarkup(person){return '<span class="avatar '+person.avatar+'">'+person.initials+'</span>'}
  function renderConversation(id){
    const d=data[id]; current=id; workspace.classList.add('chat-open');
    $$('.conversation').forEach(x=>x.classList.toggle('active',x.dataset.id===id));
    const a=$('#chatAvatar');a.className='avatar '+d.avatar;a.textContent=d.initials;
    $('#chatName').textContent=d.name;$('#chatPresence').textContent=d.presence;$('#chatTask').textContent=d.task;$('#taskLink').href='task-details-user.html?task='+encodeURIComponent(d.taskId);$('#bannerTask').textContent=d.task;$('#bannerStatus').textContent=d.status;$('#bannerStatus').className='status-pill '+(d.status==='Accepted'?'accepted':d.status==='In progress'?'accepted':'pending');document.querySelector('.task-banner small').textContent=d.banner;
    messages.innerHTML=d.messages.map(m=>m[0]==='day'?'<div class="day-divider">'+m[1]+'</div>':'<div class="bubble-row '+(m[0]==='mine'?'mine':'')+'">'+(m[0]==='them'?avatarMarkup(d):'')+'<div class="bubble"><p>'+m[1]+'</p><time>'+m[2]+'</time></div></div>').join('');
    messages.scrollTop=messages.scrollHeight;
    const convo=$('.conversation[data-id="'+id+'"]');if(convo){convo.classList.remove('unread');const badge=convo.querySelector('.conversation-meta i');if(badge)badge.remove()}
  }
  function applyFilters(){
    const q=(search.value||'').toLowerCase().trim();let visible=0;
    $$('.conversation').forEach(c=>{const matchesSearch=!q||c.dataset.search.includes(q);const matchesFilter=filter==='all'||(filter==='unread'&&c.dataset.status==='unread')||(filter==='tasks'&&!!c.dataset.task);const show=matchesSearch&&matchesFilter;c.hidden=!show;if(show)visible++});empty.hidden=visible!==0;
  }
  $$('.conversation').forEach(c=>c.addEventListener('click',()=>renderConversation(c.dataset.id)));
  search.addEventListener('input',applyFilters);
  $$('.filter').forEach(b=>b.addEventListener('click',()=>{$$('.filter').forEach(x=>x.classList.remove('active'));b.classList.add('active');filter=b.dataset.filter;applyFilters()}));
  $('#backConversations').addEventListener('click',()=>workspace.classList.remove('chat-open'));
  $('#messageForm').addEventListener('submit',e=>{e.preventDefault();const input=$('#messageInput'),value=input.value.trim();if(!value)return;const d=data[current];d.messages.push(['mine',value,new Date().toLocaleTimeString([], {hour:'numeric',minute:'2-digit'})]);renderConversation(current);input.value='';showToast('Message sent to '+d.name+'.')});
  $('#messageInput').addEventListener('input',e=>{e.target.style.height='auto';e.target.style.height=Math.min(e.target.scrollHeight,100)+'px'});
  $('#attachBtn').addEventListener('click',()=>showToast('Attachment demo — file sharing is not connected yet.'));
  $('#chatMenuBtn').addEventListener('click',()=>showToast('Conversation options: report, mute, or archive will be connected later.'));
  function openCompose(){ $('#composeModal').hidden=false }
  function closeCompose(){ $('#composeModal').hidden=true }
  $('#composeBtn').addEventListener('click',openCompose);$('#newMessageBtn').addEventListener('click',openCompose);$('#closeCompose').addEventListener('click',closeCompose);$('#cancelCompose').addEventListener('click',closeCompose);$('#composeModal').addEventListener('click',e=>{if(e.target.id==='composeModal')closeCompose()});
  $('#startCompose').addEventListener('click',()=>{const task=$('#composeTask').value;const found=Object.keys(data).find(k=>data[k].task===task)||'juan';closeCompose();renderConversation(found);showToast('Conversation opened.')});
  function showToast(msg){const t=$('#toast');if(!t)return;t.textContent=msg;t.classList.add('show');clearTimeout(window.__tt);window.__tt=setTimeout(()=>t.classList.remove('show'),2600)}
  renderConversation('juan');applyFilters();
})();
