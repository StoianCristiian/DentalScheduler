from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from typing import List, Optional
from datetime import datetime, timedelta

app = FastAPI(
    title="DentalScheduler AI",
    description="Engine pentru recomandari Smart Scheduling",
    version="1.0.0"
)

# === Modele pentru Input (Request de la .NET) ===

class TimeWindow(BaseModel):
    start_time: datetime
    end_time: datetime

class AppointmentDetails(BaseModel):
    id: str
    time_window: TimeWindow
    complexity: int = Field(default=1, description="Complexitatea procedurii, de ex. 1-5")

class SchedulingPreferences(BaseModel):
    preferred_date_start: Optional[datetime] = None
    preferred_date_end: Optional[datetime] = None
    time_of_day: Optional[str] = Field(default=None, description="'morning', 'afternoon', sau 'evening'")
    is_emergency: bool = False

class SchedulingRequest(BaseModel):
    patient_id: str
    doctor_id: str
    procedure_duration_minutes: int
    procedure_complexity: int
    doctor_availability: List[TimeWindow]
    existing_appointments: List[AppointmentDetails]
    preferences: Optional[SchedulingPreferences] = None

# === Modele pentru Output (Raspuns catre .NET) ===

class ProposedSlot(BaseModel):
    doctor_id: str
    start_time: datetime
    end_time: datetime
    score: float = Field(default=0.0, description="Cat de bine se potriveste (0-100)")
    reason: str = Field(default="", description="Explicatie scurta pentru alegere")

class SchedulingResponse(BaseModel):
    proposals: List[ProposedSlot]

# === Endpoints ===

@app.get("/health")
def health_check():
    return {"status": "healthy", "service": "DentalScheduler.AI"}

@app.post("/api/v1/schedule/recommend", response_model=SchedulingResponse)
def recommend_schedule(request: SchedulingRequest):
    """
    Primeste datele despre disponibilitatea doctorului, intalnirile existente
    si preferintele pacientului, returnand o lista de sloturi sugerate.
    """
    
    proposed_slots = []
    duration = timedelta(minutes=request.procedure_duration_minutes)
    buffer = timedelta(minutes=10) # 10 minute buffer intre programari
    step = timedelta(minutes=15) # iteram in pasi de 15 minute

    # Functie helpers pentru suprapuneri
    def is_overlapping(start: datetime, end: datetime):
        for appt in request.existing_appointments:
            appt_start = appt.time_window.start_time
            appt_end = appt.time_window.end_time
            # Daca exista orice suprapunere (tinand cont de capete)
            if start < (appt_end + buffer) and end > (appt_start - buffer):
                return True
        return False

    for avail in request.doctor_availability:
        current_time = avail.start_time
        
        while current_time + duration <= avail.end_time:
            slot_start = current_time
            slot_end = current_time + duration
            
            if not is_overlapping(slot_start, slot_end):
                # Calculam scorul pe baza preferintelor
                score = 50.0 # Baza
                reason = "Slot disponibil."
                
                if request.preferences:
                    prefs = request.preferences
                    
                    # Verificare perioada dimineata / dupa-amiaza / seara
                    hour = slot_start.hour
                    if prefs.time_of_day == 'morning' and 8 <= hour < 12:
                        score += 30
                        reason = "Se potriveste cu preferinta de dimineata."
                    elif prefs.time_of_day == 'afternoon' and 12 <= hour < 17:
                        score += 30
                        reason = "Se potriveste cu preferinta de dupa-amiaza."
                    elif prefs.time_of_day == 'evening' and 17 <= hour <= 20:
                        score += 30
                        reason = "Se potriveste cu preferinta de seara."
                        
                    # Verificare fereastra de zile (date start/end)
                    if prefs.preferred_date_start and prefs.preferred_date_end:
                        # ignoram timezone-urile complexe pentru V1
                        pref_start = prefs.preferred_date_start.replace(tzinfo=None)
                        pref_end = prefs.preferred_date_end.replace(tzinfo=None)
                        slot_start_naive = slot_start.replace(tzinfo=None)

                        if pref_start <= slot_start_naive <= pref_end:
                            score += 20
                            reason += " In perioada dorita."
                            
                    # Daca e urgenta, cel mai apropiat slot primeste scor masiv
                    if prefs.is_emergency:
                        time_to_slot = (slot_start.replace(tzinfo=None) - datetime.now()).total_seconds()
                        if time_to_slot < 0: 
                            time_to_slot = 0 # In caz ca doctor_availability e in trecut cumva
                        # Scor invers proportional cu timpul asteptat
                        bonus = max(0, 100 - (time_to_slot / 3600)) # scade 1 punct per ora distanta
                        score += bonus
                        reason = "Urgenta - Cel mai apropiat slot posibil."
                
                proposed_slots.append(ProposedSlot(
                    doctor_id=request.doctor_id,
                    start_time=slot_start,
                    end_time=slot_end,
                    score=min(score, 100.0), # cap at 100
                    reason=reason.strip()
                ))
                
            current_time += step

    # Sortam descrescator dupa scor, apoi crescator dupa ora
    proposed_slots.sort(key=lambda x: (-x.score, x.start_time))
    
    # Returnam top 3
    return SchedulingResponse(proposals=proposed_slots[:3])
