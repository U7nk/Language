#include "gc.hpp"
#include <cstdlib>

struct GCObject
{
      GCObject* next;
      bool marked;
};

GCFrame* gc_stack = nullptr;
static GCObject* heap = nullptr;

extern "C" void gc_push_frame(GCFrame* frame)
{
      frame->prev = gc_stack;
      gc_stack = frame;
}

extern "C" void gc_pop_frame()
{
      gc_stack = gc_stack->prev;
}

extern "C" void* gc_alloc(size_t size)
{
      GCObject* obj = (GCObject*)std::malloc(sizeof(GCObject) + size);

      obj->marked = false;
      obj->next = heap;
      heap = obj;

      return (void*)(obj + 1);
}

extern "C" void gc_collect()
{
      // позже добавим mark/sweep
}