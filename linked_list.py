class Link(object):
    def __init__(self, data0, next0=None, prev0=None):
        self.next = next0
        self.prev = prev0
        self.data = data0

    def insert_next(self, data0):
        my_next = self.next

        self.next = Link(data0, self.next, self)

        if my_next:
            my_next.prev = self.next

    def insert_prev(self, data0):
        my_prev = self.prev

        self.prev = Link(data0, self, my_prev)
        my_prev.next = self.prev


class LinkedList(object):
    def __init__(self):
        self.head: Link = None
        self.tail: Link = None

        self.count: int = 0

    def __iter__(self):
        return LinkedListIterator(self)

    def insert_not_sorted(self, data0):
        if self.head is None:
            self.head = self.tail = Link(data0)
        else:
            self.tail.insert_next(data0)

        self.count += 1

    def get_count(self):
        return self.count

        ##return new_link

    def any(self):
        return self.head is not None

class LinkedListIterator(object):
    def __init__(self, linked_list0: LinkedList):
        self.linked_list = linked_list0
        self.curr = self.linked_list.head

    def __next__(self):
        data0 = None

        if self.curr is not None:
            data0 = self.curr.data

            self.curr = self.curr.next
        else:
            raise StopIteration

        return data0
